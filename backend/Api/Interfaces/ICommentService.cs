using Api.DTOs.CommentDTOs;
using Api.Exceptions_i_Result_pattern;
using Api.Helpers;

namespace Api.Interfaces
{   
    // Objasnjen u IAccountService
    public interface ICommentService
    {   // Servis ne radi sa Entity klasama, vec sa DTO i samo mapira DTO<->Entity. 
        Task<List<CommentDTOResponse>> GetAllAsync(CommentQueryObject commentQueryObject, CancellationToken cancellationToken);
        Task<CommentDTOResponse> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<Result<CommentDTOResponse>> CreateAsync(string userName, string symbol, CreateCommentRequestDTO createCommentRequestDTO, CancellationToken cancellationToken);
        Task<Result<CommentDTOResponse>> DeleteAsync(int id, string userName, CancellationToken cancellationToken);
        Task<Result<CommentDTOResponse>> UpdateAsync(int id, UpdateCommentRequestDTO updateCommentRequestDTO, CancellationToken cancellationToken);
    }
}
