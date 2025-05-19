using Microsoft.AspNetCore.Identity;

namespace Api.Models
{   
    // Zbog logovanja na App kroz AddIdentity u Program.cs
    public class AppUser : IdentityUser // AspNetUsers tabela
    {   
        // U ApplicationDbContext OnModelCreating definisem PK(AppUser.Id) vezu sa FK iz Porftolio (AppUserId)
        public List<Portfolio> Portfolios { get; set; } = new List<Portfolio>(); // Navigation property needs default value da ne bude NULL reference error jer necemo prosledjivati nikad ovo polje prilikom kreiranja ove klase
        /* Obzirom da AppUser moze imati vise Stocks, nije dobra prkasa da ovde ima polje List<Stock> Stocks, vec svaki taj Stock od AppUser predstavljam kao Portfolio,
        a sve zajedno u List<Portfolio>.
         Nece postojati kolona Portfolios u AspNetUsers tabeli, jer nije Primary type lista, pa zato moram imati FK-PK relaciju za AppUser-Portfolio klase tj u Portfolio klasi
        imam AppUserId polje koje je FK za ovaj PK ovde (Id polje iz IdentityUser). U OnModelCreating, za Portfolios tabelu, sam definisao relacije izmedju Portfolio-AppUser klasa.
        Kad radim GET request, trebace mi Include ako zelim i Portfolios da dohvatim, jer Navigation property nije automatski ocitan, jer usporava rad aplikacije, pa moram explicitno da ga definisem.*/
    }
}
