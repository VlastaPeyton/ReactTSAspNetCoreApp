namespace Api.DTOs.Account
{   
    // Nema Data Validation, jer sluzi za prikazivanje usera samo 
    public class NewUserDTO
    {
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Token { get; set; }
    }
}
