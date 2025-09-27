using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account
{    /* Ovog redosleda i imena navodim arugmente u React request kad pozivam ForgotPassword endpoint
        Zbog Data Validation mi treba, jer ModelState u ForgotPassword endpoint na osnovu ovih annotations automatski radi validaciju
        Request DTO se koristi kad FE poziva endpoint, jer ne sme Models (entity) klasu koristiti u tom slucaju, jer ona sluzi samo za DB interaction u Repository
    */
    public class ResetPasswordDTO
    {
        [Required]
        public string NewPassword { get; set; }
        [Required]
        public string ResetPasswordToken { get; set; }
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }
}
