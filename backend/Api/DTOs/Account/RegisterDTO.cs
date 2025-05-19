using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account
{   

    // Zbog Data Validation mi treba
    public class RegisterDTO
    {   // Ovog redosleda i imena navodim argumente kad gadjam register endpoint

        [Required]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
