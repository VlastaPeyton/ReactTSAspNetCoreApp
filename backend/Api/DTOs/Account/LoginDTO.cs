using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account
{   
    // Ovog redosleda i imena navodim arugmente u React kad gadjam login endpoint
    public class LoginDTO
    {   
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
