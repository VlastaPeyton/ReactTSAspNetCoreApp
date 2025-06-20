using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account
{   

    // Zbog Data Validation mi treba, jer ModelState u Register endpoint, na osnovu ovih annotations automatski radi validaciju
    public class RegisterDTO
    {   // Ovog redosleda i imena navodim argumente kad gadjam Register endpoint 

        [Required]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [Required]
        public string? Password { get; set; }
    }
    /* U Register endpoint, ModelState ce da proveri da li je unesen UserName zbog [Required], da li je EmailAddress unesen i da li je zeljenog oblika,
     da li je Password unesen. */
}
