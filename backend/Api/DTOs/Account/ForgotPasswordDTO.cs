using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account

{   // Ovog redosleda i imena navodim arugmente u React kad gadjam ForgotPassword endpoint
    // Zbog Data Validation mi treba, jer ModelState u ForgotPassword endpoint, na osnovu ovih annotations automatski radi validaciju
    // DTO je request object koji se koristi kad FE gadja Endpoint jer ne moze Entity klasa jer ona sluzi za DB interaction u Repository
    public class ForgotPasswordDTO
    {

        [Required]
        [EmailAddress]
        public string? EmailAddress { get; set; }
    }
}
