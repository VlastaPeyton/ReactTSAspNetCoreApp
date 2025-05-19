using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.CommentDTOs
{   
    // Objasnjeno u CommentDTO cemu sluzi DTO.
    public class UpdateCommentRequestDTO
    {

        [Required]
        [MinLength(5, ErrorMessage = "Title must be at least 5 chars")]
        [MaxLength(200, ErrorMessage = "Title cannot be over 200 chars")]
        // Ove 3 linije iznad su Data Validation za Title kolonu
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(5, ErrorMessage = "Content must be at least 5 chars")]
        [MaxLength(200, ErrorMessage = "Content cannot be over 200 chars")]
        public string Content { get; set; } = string.Empty;
    }
}
