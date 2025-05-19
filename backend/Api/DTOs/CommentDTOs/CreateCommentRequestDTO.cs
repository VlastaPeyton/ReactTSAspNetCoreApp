using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.CommentDTOs
{   
    // Samo ova polja iz Comment sam uzeo, a mogo sam i ostala mada imaju default vrednosti, pa nisam

    // Objasnjeno u CommentDTO cemu DTO sluzi.
    public class CreateCommentRequestDTO
    {   
        [Required]
        [MinLength(5,ErrorMessage = "Title must be at least 5 chars")]
        [MaxLength(200, ErrorMessage = "Title cannot be over 200 chars")]
        // Ove 3 linije iznad su Data Validation za Title kolonu
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(5, ErrorMessage = "Content must be at least 5 chars")]
        [MaxLength(200, ErrorMessage = "Content cannot be over 200 chars")]
        public string Content { get; set; } = string.Empty;
    }
}
