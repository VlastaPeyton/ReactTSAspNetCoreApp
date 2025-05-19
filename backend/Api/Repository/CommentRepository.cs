using Api.Data;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository
{
    /* Repository pattern kako bi, umesto u CommentController, u COmmentRepository definisali tela Endpoint metoda + DB calls u Repository se stavljaju i zato
    ovde moze ne ide CommentDTO, vec samo Comment. 
       Moze u UpdateAsync, umesto Comment da bude UpdateCommentRequestDTO, ali sam u CommentkMapper napravio Extension Method "ToCommentFromUpdateCommentRequestDTO", jer 
    Repository interaguje sa bazom i ne zelim da imam DTO klase ovde.*/
    public class CommentRepository : ICommentRepository
    {   
        private readonly ApplicationDBContext _dbContext;
        public CommentRepository(ApplicationDBContext context)
        {
            _dbContext = context;
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            await _dbContext.Comments.AddAsync(comment);
            await _dbContext.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> DeleteAsync(int id)
        {
            var comment = await _dbContext.Comments.FirstOrDefaultAsync(c =>  c.Id == id);
            if (comment is null)
                return null;

            _dbContext.Comments.Remove(comment); // Remove nema async
            await _dbContext.SaveChangesAsync();

            return comment;
        }

        public async Task<List<Comment>> GetAllAsync(CommentQueryObject commentQueryObject)
        {
            var comments = _dbContext.Comments.Include(c => c.AppUser).AsQueryable(); // AsQueryable kako bi mogo da nastavim LINQ nad comments
            if (!string.IsNullOrWhiteSpace(commentQueryObject.Symbol))
                comments = comments.Where(s => s.Stock.Symbol == commentQueryObject.Symbol);
            
            if(commentQueryObject.IsDescending == true)
                comments = comments.OrderByDescending(c => c.CreatedOn); 

            return await comments.ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            var existingComment = await _dbContext.Comments.Include(c => c.AppUser).FirstOrDefaultAsync(c => c.Id == id); // FindAsync pretrazuje samo by Id, ali ne moze ovde jer ima Include
            if (existingComment is null)
                return null;

            return existingComment;
        }

        public async Task<Comment?> UpdateAsync(int id, Comment comment)
        {
            var existingComment = await _dbContext.Comments.FindAsync(id); // Brze nego FirstOrDefaultAsync jer moze ovo
            if (existingComment is null)
                return null;

            // Azuriram samo polja navedena u UpdateCommentRequestDTO
            existingComment.Title = comment.Title;
            existingComment.Content = comment.Content;

            await _dbContext.SaveChangesAsync();

            return existingComment;
        }
    }
}
