namespace Api.DTOs.Account
{   
    // Nema Data Validation, jer sluzi za slanje UserName, EmailAddress i Token kada se novi user registruje u Register endpoint 
    // Ovu klasu saljem to FE jer se iz Endpoint to FE samo DTO klase salju a ne Entity jer one sluze za Repository i work with DB
    public class NewUserDTO
    {
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Token { get; set; }
    }
}
