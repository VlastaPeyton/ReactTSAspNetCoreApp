using Api.Helpers;
using Api.Models;

namespace Api.Interfaces
{
    /* Repository pattern kako bi, umesto u CommentController, u CommentRepository definisali tela Endpoint metoda tj DB calls smestili u Repository.
       Koriscenje ICommentRepository omogucava testiranje bez koriscenja baze + loose coupling.
       Ne koristim CommentDTO, vec Comment, jer Repository direktno sa bazom komunicira. */
    public interface ICommentRepository
    {
        // Task, jer u CommentRepository bice definisane kao async + ce u CommentController Endpoint mozda da ih poziva pomocu await
        // Metoda koja ima Comment?, zato sto compiler warning prikaze ako method's return moze biti null jer FirstOrDefault/FindAsync moze i null da vrati
        Task<List<Comment>> GetAllAsync(CommentQueryObject commentQueryObject);
        Task<Comment?> GetByIdAsync(int id);
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment?> DeleteAsync(int id); 
        Task<Comment?> UpdateAsync(int id, Comment comment);
    }
}
