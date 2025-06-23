using Api.Data;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository
{
    /* Repository pattern kako bi, umesto u CommentController, u CommentRepository definisali tela Endpoint metoda + DB calls u Repository se stavljaju i zato
    ovde moze ne ide CommentDTO, vec samo Comment jer Models Entity klase se koriste za EF Core.
               
       Repository interaguje sa bazom i ne zelim da imam DTO klase ovde, vec Entity klase i zato u Controller koristim mapper extensions da napravim Entity klasu from DTO klase.
    
       Objasnjenje za CancellationToken pogledaj u CommentController. 
    */
    public class CommentRepository : ICommentRepository
    {   
        private readonly ApplicationDBContext _dbContext;
        public CommentRepository(ApplicationDBContext context)
        {   
            _dbContext = context;
        }

        // Sve metode su async, jer u StockController bice pozvace pomocu await. 
        // Metoda koja ima Comment?, zato sto compiler warning prikaze ako method's return moze biti null jer FirstOrDefault/FindAsync moze i null da vrati

        public async Task<Comment> CreateAsync(Comment comment, CancellationToken cancellationToken)
        {
            await _dbContext.Comments.AddAsync(comment, cancellationToken); // EF start tracking comment object changes => Ako baza uradi nesto u vrsti koja predstavlja comment, EF to aplikuje u comment, i obratno 
            // EF change tracker marks comment tracking state to Added 
            await _dbContext.SaveChangesAsync(cancellationToken); // DB doda vrednost u Id column for row corresponding to comment object => EF updates Id field in comment object
            return comment;
        }

        public async Task<Comment?> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var comment = await _dbContext.Comments.FirstOrDefaultAsync(c =>  c.Id == id, cancellationToken); // EF start tracking comment object, so every change made to comment will be applied to its corresponding row in Comment table after SaveChangesAsync
            if (comment is null)
                return null;

            _dbContext.Comments.Remove(comment); // Remove nema async stoga ni cancellationToken.  EF in Change Tracker marks comment tracking state to Deleted
            await _dbContext.SaveChangesAsync(cancellationToken); // comment is no longer tracked by EF

            return comment;
        }

        public async Task<List<Comment>> GetAllAsync(CommentQueryObject commentQueryObject, CancellationToken cancellationToken)
        {
            var comments = _dbContext.Comments.Include(c => c.AppUser).AsQueryable(); 
            // Comment ima AppUser polje i PK-FK vezu sa AppUser i zato moze Include
            // AsQueryable zadrzava LINQ osobine, pa mogu kasnije npr comments.Where(...)
            // Ovde nema EF tracking jer nisam izvuko 1 row iz Comments tabele, vec sve 

            // In if statement no need to AsQueryable again
            if (!string.IsNullOrWhiteSpace(commentQueryObject.Symbol))
                comments = comments.Where(s => s.Stock.Symbol == commentQueryObject.Symbol);
                
            if(commentQueryObject.IsDescending == true)
                comments = comments.OrderByDescending(c => c.CreatedOn); 

            return await comments.ToListAsync(cancellationToken);
        }

        public async Task<Comment?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {   // FindAsync pretrazuje samo by Id i brze je od FirstOrDefaultAsync, ali ne moze ovde jer ima Include, pa mora FirstOrDefaultASync
            var existingComment = await _dbContext.Comments.Include(c => c.AppUser).FirstOrDefaultAsync(c => c.Id == id, cancellationToken); 
            // EF start tracking changes done in existingComment after FirstOrDefaultAsync, ali ovde ne menjam nista u objektu
            if (existingComment is null)
                return null;

            return existingComment;
        }

        public async Task<Comment?> UpdateAsync(int id, Comment comment, CancellationToken cancellationToken)
        {
            var existingComment = await _dbContext.Comments.FindAsync(id, cancellationToken); // Brze nego FirstOrDefaultAsync jer moze ovo
            // EF starts tracking changes in existingComment object, so any changes made to existingComment will be applied to its corresponding row in DB after SaveChangesAsync
            if (existingComment is null)
                return null;

            // Azuriram samo polja navedena u UpdateCommentRequestDTO
            existingComment.Title = comment.Title;
            existingComment.Content = comment.Content;

            await _dbContext.SaveChangesAsync(cancellationToken); // Apply changes in DB row

            return existingComment;
        }
    }
}
