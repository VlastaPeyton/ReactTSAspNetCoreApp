namespace Api.DTOs.Account
{   
    // Nema Data Validation, jer sluzi za slanje UserName, EmailAddress i Token kada se novi user registruje u Register endpoint 
    public class NewUserDTO
    {
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Token { get; set; }
    }
}
