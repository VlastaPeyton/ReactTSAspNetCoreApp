using Microsoft.AspNetCore.Identity;

namespace Api.Models
{   
    // Models folder sluzi za Entity klase jer te klase ce biti tabele u bazi. 

    // Zbog logovanja na App kroz AddIdentity u Program.cs mi treba ova klasa.
    public class AppUser : IdentityUser // AspNetUsers tabela koja se automatski kreira u bazi i odnosice se na AppUser.
    {    
        /* AppUser nasledio sva polja iz IdentityUser, gde najvise nas zanima Id, UserName, Email i Password polje. 
         Ako dodam custom field u AppUser (ali da nije Navigation attribute), to polje ce biti dodatna kolona u AspNetUsers table
         
          Zbog IdentityUser definicije, "ne mogu" AppUser:IdentityUser<AppUserId>, gde AppUserId je Value Object, jer morao bih onda AppUserId:IEquatable sto je cimanje. 
        Ovako, by default, Id je string.
        */

        // U ApplicationDbContext OnModelCreating definisem PK(AppUser.Id) vezu sa FK iz Porftolio.cs (AppUserId) mada to bi EF i sam znao 
        public List<Portfolio> Portfolios { get; set; } = new List<Portfolio>(); // Collection navigation property. Dobra praksa da ima default value. Uz PK-FK za AppUser-Portfolio, omogucava koriscenje Include sto olaksava LINQ (Eager loading).

        /* Obzirom da AppUser moze imati vise Stocks, nije dobra prkasa da AppUser ima polje List<Stock> Stocks, vec svaki taj Stock od AppUser predstavljam kao Portfolio (npr Portfolio1 = {AppUserId1, StockId1, AppUser1, Stock1}), a sve Stocks od AppUser predstavlja kao List<Portfolio>.
         
           Navigation property : - Moze biti tipa druge Entity klase ili List<DrugaEntityKlasa>. Ako je List, onda dobra praksa dodati mu default value da compiler ne kuka.
                                 - Ako je tipa druge Entity klase, a ta klasa ima navigation property tipa ove klase, mora u Program.cs se napisati ona JSON fora da gasi circular reference
                                 - U OnModelCreating obavezno PK-FK definicija (ili EF na osnovu imena PK/FK polja i Navigation property moze to i sam na osnovu imena polja)
                                 - Kad FE gadja GET Endpoint, trebace mi Include (Eager loading) ako zelim i Portfolios da dohvatim, jer ovaj Portfolios Navigation property nije automatski ocitan, jer usporava rad aplikacije, pa moram explicitno da ga ocitam pomocu Include.
                                 - Olaksava LINQ, jer uz definisanu FK-PK relaciju, mogu koristiti Include u LINQ da kroz ovu klasu dohvatim njen navigation property 
                                 - Ne postoji kao kolona u tabeli jer ne moze kolona biti non primary typa 
                                 - Kada writing to DB, from FE ne saljemo nikad ovo polje, jer ono, uz PK i FK polja, sluzi da poveze 2 tabele. I zato cesto mora imati default vrednost 

          Ovo je 1-to-many AppUser-Portfolio veza, jer AppUser ima List<Portfolio> dok Portfolio ima AppUser i AppUserId polje, ali EF NECE zakljucit ovu vezu sam, jer u Portfolio neam Id polje, vec ga pravim od AppUserId+StockId, pa zato u OnModelCreating definisem taj composite PK i ovu vezu.
          Da je Portfolio.cs imao List<AppUser> AppUsers polje, ovo bi bilo many-to-many veza.
         
          Nece postojati kolona Portfolios u AspNetUsers tabeli, jer nije Primary type lista. 
         */
    }
}
