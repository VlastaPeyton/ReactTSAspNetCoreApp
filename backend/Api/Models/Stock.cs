using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    // Stock table i ovo se koristi samo za EF tj bazu. Za Endpoints se koristi StockDTO
    [Table("Stocks")]
    public class Stock // Entity jer predstavlja tabelu u bazi
    {
        public int Id { get; set; } // PK 

        // Svaki Stock se prepoznaje po symbol npr Microsoft = MSFT, a Tesla = TSLA
        public string Symbol { get; set; } = string.Empty; // Ako ne unesem nista, u koloni Symbol bice prazan string 
        public string CompanyName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]  // Tip kolone "Purchase" u tabeli 
        public decimal Purchase {  get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Dividend { get; set; }

        public string Industry { get; set; } = string.Empty;

        public long MarketCap { get; set; }

        public List<Comment> Comments { get; set; } = new();  // Navigation property needs default value to avoin null reference exception jer necu proslediti nikad ovo polje prilikom kreiranja objekta
        /* Nece postojati kolona Comments u Stocks tabeli, jer nije Primary type lista, pa zato moram imati FK-PK relacju za Stock-Comment klase tj u Comment klasi 
         imam StockId polje koje je FK za ovaj PK ovde. EF Core will establish one-to-many relationship izmedju Stock i Comment tj 1 Stock imace vise Comments.
           U StockRepository, EF Core nece automatski da ocita Comments osim ako to explicitno ne uradim pomocu Include, jer Navigation Property nije automatski ocitan 
        nikad, jer on usporava rad aplikacije, i moram da naglasim da zelim i Comments ako ih zaista zelim + zato sto nisam u OnModelCreating definisao relaciju za Stock-Comment. */

        public List<Portfolio> Portfolios { get; set; } = new List<Portfolio>(); // Navigation property needs default value da ne bude NULL reference error 
        /* Obzirom da vise AppUsers moze imati isti Stock, nije dobra praksa da Stock ima List<AppUser> polje, vec Portfolio klasom predstavim svaki AppUser koji ima taj Stock. 
         Nece postojati kolona Portfolios kolona u Stocks tabeli, jer nije Primary type lista, pa zato moram imati FK-PK relaciju za Stock-Portfolio klase tj u Portfolio klasi
        imam StockId polje koje je FK za ovaj PK ovde. U OnModelCreating, za Portfolios tabelu, sam definisao relacije izmedju Portfolio-Stock klasa. Za GET metodu, treba Include jer je ovo
        Navigation property, pa EF Core nece automatski da ocita Portfolios, osim ako ne kazem explicitno with Include.*/
    }
}
