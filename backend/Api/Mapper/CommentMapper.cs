using System.Reflection;
using System.Runtime.CompilerServices;
using Api.DTOs.CommentDTOs;
using Api.Models;

namespace Api.Mapper
{   
    // Da napravim Comment Entity klasu od DTO klasa kada pokrecem Repository metode.
    // Da napravim DTO klasu od Comment Entity klasa kada saljem data to FE in Endpoint
    public static class CommentMapper
    {   
        // Extension method za Comment, without calling parameters
        public static CommentDTO ToCommentDTO(this Comment comment)
        {
            return new CommentDTO
            {
                Id = comment.Id.Value, // Id mapiram jer mapiram iz Comment to CommentDTO. Id u Comment ima Value polje.
                Title = comment.Title,
                Content = comment.Content,
                CreatedOn = comment.CreatedOn,
                StockId = comment.StockId,
                CreatedBy = comment.AppUser.UserName
                /* Ne mapiram AppUser,AppUserId,Stock,StockId polja, jer nisu prisutna u CommentDTO, dok AppUser/Stock su u Comment navigation property, koja, uz PK i FK polja u Comment/AppUser/Stock, sluze
                 da EF (ili ja u OnModelCreating) definsie PK-FK vezu za Comment-AppUser/Stock. */
            };
        }

        // Extension method za CreateCommentRequestDTO, with calling parameter
        public static Comment ToCommentFromCreateCommentRequestDTO(this CreateCommentRequestDTO createCommentRequestDTO, int stockId)
        {
            return new Comment
            {
                // Id se ne mapira iz DTO,jer DTO nema Id, jer to tabela sama dodeli prema OnModelCreating zbog ValueGeneratedOnAdd za custom CommentId type
                Title = createCommentRequestDTO.Title,
                Content = createCommentRequestDTO.Content,
                StockId = stockId,
                // CreatedOn polje nisam mapirao, jer ne postoji u CreateCommentRequestDTO, pa bice DateTime.Now by default 
                /* Ne mapiram AppUser i Stock polja, jer nisu prisutna u CreateCommentRequestDTO, jer su u Comment to navigation property, koja, uz PK i FK polja u Comment/AppUser/Stock, sluze
                 da EF (ili ja u OnModelCreating) definsie PK-FK vezu za Comment-AppUser/Stock. */
            };
        }

        // Extension method za UpdateCommentRequestDTO, without calling parameters
        public static Comment ToCommentFromUpdateCommentRequestDTO(this UpdateCommentRequestDTO updateCommentRequestDTO)
        {
            return new Comment
            {
                Title = updateCommentRequestDTO.Title,
                Content = updateCommentRequestDTO.Content,
                // Ostala non navigational property ili Id polja nisam mapirao jer to ne treba za ovaj slucaj  + UpdateCommentRequestDTO samo ova 2 polja ima
                /* Ne mapiram AppUser i Stock polja, jer nisu prisutna u UpdateCommentRequestDTO, jer su u Comment navigation property, koja, uz PK i FK polja u Comment/AppUser/Stock, sluze
                 da EF (ili ja u OnModelCreating) definsie PK-FK vezu za Comment-AppUser/Stock. */
            };
        }
    }
}
