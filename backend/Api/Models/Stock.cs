using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    // Stock table i ovo se koristi samo za EF tj bazu. Za Endpoints se koristi StockDTO jer Stock se koristi samo u ApplicaitonDbContext kad interaguje sa bazon kroz EF.
    [Table("Stocks")]
    public class Stock // Entity jer predstavlja tabelu u bazi
    {
        public int Id { get; set; } // PK 

        // Svaki Stock se prepoznaje po symbol (npr Microsoft = MSFT, a Tesla = TSLA)
        public string Symbol { get; set; } = string.Empty; // Ako ne unesem nista, u koloni Symbol bice prazan string 
        public string CompanyName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]  // Tip kolone "Purchase" u tabeli tj koliko max brojeva moze i koliko iza zareza
        public decimal Purchase {  get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Dividend { get; set; }

        public string Industry { get; set; } = string.Empty;

        public long MarketCap { get; set; }

        // U FE, search stock by ticker, and selecting on it, i can leave a comment for that stock
        public List<Comment> Comments { get; set; } = new();  // Navigation property needs default value to avoin null reference exception jer necu proslediti nikad ovo polje prilikom kreiranja objekta jer ova kolona ne moze postojati u Stocks tabeli
        /* Nece postojati kolona Comments u Stocks tabeli, jer nije Primary type lista, pa zato moram imati FK-PK relacju za Stock-Comment klase tj u Comment.cs
         imam StockId polje koje je FK za ovaj PK ovde. EF Core will establish one-to-many relationship izmedju Stock i Comment tj 1 Stock imace vise Comments.
           U StockRepository, EF Core nece automatski da ocita Comments osim ako to explicitno ne uradim pomocu Include, jer Navigation Property nije automatski ocitan 
        nikad, jer on usporava rad aplikacije, i moram da naglasim da zelim i Comments ako ih zaista zelim + zato sto nisam u OnModelCreating definisao relaciju za Stock-Comment. */

        public List<Portfolio> Portfolios { get; set; } = new List<Portfolio>(); // Navigation property needs default value da ne bude NULL reference error  jer nikad ovu vrednost necu proslediti pri kreiranju objekta jer ova kolona ne moze postojati u Stocks tabeli
        /* Obzirom da vise AppUsers moze imati isti Stock, nije dobra praksa da Stock ima List<AppUser> polje, vec Portfolio klasom predstavim svaki AppUser koji ima taj Stock. Portfolio ovde koristim kontra od onoga u AppUser.
         
        Nece postojati kolona Portfolios kolona u Stocks tabeli, jer nije Primary type lista. Obzriom da je Portfolios lista, moram imati FK-PK relaciju za Stock-Portfolio klase tj u Portfolio.cs
        imam StockId polje koje je FK za ovaj PK ovde. U OnModelCreating, za Portfolios tabelu, sam definisao relacije izmedju Portfolio-Stock. 
        
        Portfolio.cs ima Stock polje i zato EF zna da je ovo 1-to-many (1 Stock can belong to Many Portfolios), ali ovaj List<Portfolio> stoji kako bih mogo da dohvatim i Portfolio preko Stocks tabele tj mogu koristiti INCLUDE u LINQ.

        Navigation property : - Je polje u Stock Entity klasi koje je tipa druge Entity klase (Portfolios/Comments u ovom slucaju),
                                gde Portfolio/Comment mora imati StockId FK za koje ce, u OnModelCreating, da se definise veza sa Id iz Stock.  
                                -  Kad radim GET request tj gadjam GET Endpoint, trebace mi Include ako zelim i Portfolios/Comments da dohvatim, jer ovaj Portfolios Navigation property nije automatski ocitan, jer usporava rad aplikacije, pa moram explicitno da ga ocitam pomocu Include.
      */
    }
}
 