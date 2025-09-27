using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account
{
    /* Ovog redosleda i imena navodim argumente u React request kad pozivam Register endpoint
       Zbog Data Validation mi treba, jer ModelState u Register endpoint na osnovu ovih annotations automatski radi validaciju
       Request DTO se koristi kad FE poziva endpoint, jer ne sme Models (entity) klasu koristiti u tom slucaju, jer ona sluzi samo za DB interaction u Repository
    */
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
}
