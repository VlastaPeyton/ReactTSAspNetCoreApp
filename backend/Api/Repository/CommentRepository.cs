using Api.Data;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository
{
    /* Repository pattern kako bi, umesto u CommentController, u CommentRepository definisali tela endpoint metoda + DB calls u Repository se stavljaju i zato
    ovde ne ide CommentDTO, vec samo Comment, jer (Models) Entity klase se koriste za EF Core.
               
       Repository interaguje sa bazom i ne zelim da imam DTO klase ovde, vec Entity klase koje predstavljaju tabele i zato u Controller koristim mapper extensions da napravim Entity klasu from DTO klase.
    
       Objasnjenje za CancellationToken pogledaj u CommentController. 
    */
    public class CommentRepository : ICommentRepository
    {   
        private readonly ApplicationDBContext _dbContext;
        public CommentRepository(ApplicationDBContext context)
        {   
            _dbContext = context;
        }

        /* Sve metode su async, jer u StockController bice pozvace pomocu await. 
           Metoda koja ima Task<Comment?>, zato sto compiler warning ce prikaze ako method's return moze biti null jer FirstOrDefault/FindAsync moze i null da vrati
          Ovo je dobra praksa da compiler ne prikazuje warning.
         */
        public async Task<Comment> CreateAsync(Comment comment, CancellationToken cancellationToken)
        {
            await _dbContext.Comments.AddAsync(comment, cancellationToken); 
            /* EF start tracking comment object => Ako baza uradi nesto u vrsti koja predstavlja comment, EF to aplikuje u comment, i obratno 
             EF change tracker marks comment tracking state to Added. Ne smem AsNoTracking, jer AddAsync(comment) nece hteti ako entity object nije tracked.
            */
            await _dbContext.SaveChangesAsync(cancellationToken); // DB doda vrednost u Id column for row corresponding to comment object => EF updates Id field in comment object
            return comment;
        }

        public async Task<Comment?> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var comment = await _dbContext.Comments.FirstOrDefaultAsync(c => c.Id.Value == id, cancellationToken);  // Jer Id field of Comment ima Value polje int tipa
            /* EF start tracking comment object, so every change made to comment will be applied to its corresponding row in Comment table after SaveChangesAsync
             Ne smem AsNoTracking, jer Remove(comment) nece hteti ako entity object nije tracked.
            
             Id je PK i Index, tako da FirstOrDefaultAsync u O(1)(ako je Recnik struktura indexa) ili O(logn) (ako je B-tree struktura indexa) nadje zeljeni komentar.
            */

            if (comment is null)
                return null;

            _dbContext.Comments.Remove(comment); // Remove nema async, stoga nema ni cancellationToken.  EF in Change Tracker marks comment tracking state to Deleted
            await _dbContext.SaveChangesAsync(cancellationToken); // comment is no longer tracked by EF

            return comment;
        }

        public async Task<List<Comment>> GetAllAsync(CommentQueryObject commentQueryObject, CancellationToken cancellationToken)
        {
            var comments = _dbContext.Comments.AsNoTracking().Include(c => c.AppUser).AsQueryable();  // Include is Eager loading
            // Comment ima AppUser polje i PK-FK vezu sa AppUser, pa zato moze Include(c => c.AppUser)
            // AsQueryable zadrzava LINQ osobine, pa mogu kasnije npr comments.Where(...), comments.OrderByDescending(...) itd.
            // Ovde nema EF tracking jer sam stavio AsNoTracking posto necu da modifikujem/brisem comments nakon ocitavanja iz baze, pa da ne dodajem overhead and memory zbog tracking

            // In if statement no need to AsQueryable again
            if (!string.IsNullOrWhiteSpace(commentQueryObject.Symbol))
                comments = comments.Where(s => s.Stock.Symbol == commentQueryObject.Symbol); 
                // Ovaj Endpoint koristim cesto jer gledam Company profile za zeljeni Stock, a ta stranica ocitava sve komentare za njega, pa Stock.Symbol sam stavio ko Index da brze ocitava - pogledaj OnModelCreating
                
            if(commentQueryObject.IsDescending == true)
                comments = comments.OrderByDescending(c => c.CreatedOn); 

            return await comments.ToListAsync(cancellationToken); // Mora ToListAsync, jer comments je AsQueryable (LINQ tipa tj SQL)
        }

        public async Task<Comment?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {   // FindAsync pretrazuje samo by Id i brze je od FirstOrDefaultAsync, ali ne moze ovde jer ima Include, pa mora FirstOrDefaultASync
            var existingComment = await _dbContext.Comments.AsNoTracking().Include(c => c.AppUser).FirstOrDefaultAsync(c => c.Id.Value == id, cancellationToken); // Jer Id of Comment ima Value polje int tipa
            // Id je PK i Index tako da pretrazuje bas brzo O(1) ili O(logn) u zavisnosti koja je struktura za index uzeta
            // EF start tracking changes done in existingComment after FirstOrDefaultAsync, ali ovde ne menjam/brisem objekat pa sam dodao AsNoTracking jer tracking dodaje overhead and uses memory
            if (existingComment is null)
                return null;

            return existingComment;
        }

        public async Task<Comment?> UpdateAsync(int id, Comment comment, CancellationToken cancellationToken)
        {
            var existingComment = await _dbContext.Comments.FindAsync(id, cancellationToken); // Brze nego FirstOrDefaultAsync + samo za Id polje moze
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
