using Api.DTOs.CommentDTOs;

namespace Api.CQRS_and_Validation.Comment.Delete
{   
    //public record CommentDeleteRequest(int Id, string userName); // Ne treba mi, jer samo jedan prostog tipa argument ima u Delete endpoint
    //public record CommentDeleteResponse(CommentDTOResponse commentDTOResponse); // Ne treba mi ni ovo - isti razlog kao kod CommentGetByIdResponse
}
