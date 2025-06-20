﻿using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account
{
    // Ovog redosleda i imena navodim arugmente u React kad gadjam Login endpoint
    // Zbog Data Validation mi treba, jer ModelState u Login endpoint, na osnovu ovih annotations automatski radi validaciju

    public class LoginDTO
    {   
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
    /* U Login endpoint, ModelState ce da proveri da li je unesen UserName/Password zbog [Required]  */
}
