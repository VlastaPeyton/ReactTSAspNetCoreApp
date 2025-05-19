using System.Reflection;
using System.Runtime.CompilerServices;
using Api.DTOs.CommentDTOs;
using Api.Models;

namespace Api.Mapper
{
    public static class CommentMapper
    {   
        // Extension method za Comment, without calling parameters
        public static CommentDTO ToCommentDTO(this Comment comment)
        {
            return new CommentDTO
            {
                Id = comment.Id, // Id mapiram jer mapiram iz Comment to DTO
                Title = comment.Title,
                Content = comment.Content,
                CreatedOn = comment.CreatedOn,
                StockId = comment.StockId,
                CreatedBy = comment.AppUser.UserName
            };
        }

        // Extension method za CreateCommentRequestDTO, with calling parameter
        public static Comment ToCommentFromCreateCommentRequestDTO(this CreateCommentRequestDTO createCommentRequestDTO, int stockId)
        {
            return new Comment
            {   
                // Id se ne mapira iz DTO,jer DTO nema Id, jer to tabela sama dodeli
                Title = createCommentRequestDTO.Title,
                Content = createCommentRequestDTO.Content,
                StockId = stockId,
                // Stock polje nisam mapirao, jer ne postoji u CreateCommentRequestDTO, pa bice NULL by default
                // CreatedOn polje nisam mapirao, jer ne postoji u CreateCommentRequestDTO, pa bice DateTime.Now by default 
                // CreatedBy polje nisam mapirao, jer ne postoji u CreateCommentRequestDTO, pa bice default null
            };
        }

        // Extension method za UpdateCommentRequestDTO, without calling parameters
        public static Comment ToCommentFromUpdateCommentRequestDTO(this UpdateCommentRequestDTO updateCommentRequestDTO)
        {
            return new Comment
            {
                Title = updateCommentRequestDTO.Title,
                Content = updateCommentRequestDTO.Content,
            };
        }
    }
}
