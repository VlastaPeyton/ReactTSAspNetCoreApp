using Microsoft.AspNetCore.Identity;

namespace Api.Models
{   
    // Models folder sluzi za Entity klase jer te klase ce biti tabele u bazi. 
    // Zbog logovanja na App kroz AddIdentity u Program.cs
    public class AppUser : IdentityUser // AspNetUsers tabela koja se automatski kreira u bazi i odnosice se na AppUser.
    {    
        /* AppUser nasledio sva polja iz IdentityUser, gde najvise nas zanima Id, UserName, Email i Password polje. 
         Ako dodam custom field u AppUser (ali da nije Navigation attribute of reference type), to polje ce biti dodatna kolona u AspNetUsers table
        */

        // U ApplicationDbContext OnModelCreating definisem PK(AppUser.Id) vezu sa FK iz Porftolio.cs (AppUserId)
        public List<Portfolio> Portfolios { get; set; } = new List<Portfolio>(); // Navigation property needs default value da ne bude NULL reference error jer necemo prosledjivati nikad ovo polje prilikom kreiranja ove klase obzirom da ne moze postojati ova kolona u tabeli
        /* Obzirom da AppUser moze imati vise Stocks, nije dobra prkasa da AppUser ima polje List<Stock> Stocks, vec svaki taj Stock od AppUser predstavljam kao Portfolio, a sve Stocks predstavlja kao List<Portfolio>.
         
           Nece postojati kolona Portfolios u AspNetUsers tabeli, jer nije Primary type lista. Obzirom da je Portfolios lista, moram imati FK-PK relaciju za AppUser-Portfolio tj u Portfolio.cs
        imam AppUserId polje koje je FK za ovaj PK ovde (Id polje iz IdentityUser). U OnModelCreating, za Portfolios tabelu, sam definisao relacije izmedju Portfolio-AppUser.
           
           Navigation property : - Je polje u Entity klasi (AppUser je entity jer zbog njega se kreira AspNetUsers tabela) koje je tipa druge Entity klase (Portfolio.cs u ovom slucaju).  
                                gde Portfolio.cs mora imati AppUserId FK za koje ce, u OnModelCreating, da se definise veza sa Id iz AppUser.   
                                -  Kad radim GET request tj gadjam GET Endpoint, trebace mi Include ako zelim i Portfolios da dohvatim, jer ovaj Portfolios Navigation property nije automatski ocitan, jer usporava rad aplikacije, pa moram explicitno da ga ocitam pomocu Include.
            
           Portfolio.cs ima AppUser polje i zato EF zna da je ovo 1-to-many (1 AppUser can have Many Portfolios), ali ovaj List<Portfolio> stoji kako bih lakse dohvaito i Portfolio preko AspNetUsers tabele tj da mogu koristit INCLUDE u LINQ
         */
    }
}
