using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    /*Join table koja ce da predstavlja each Stock of each AppUser (zato Stock polje postoji), jer nije dobra praksa da AppUser ima Stocks listu kao polje i Stock da ima AppUsers listu kao polje.
     Takodje, predstavlja AppUser of each Stock i zato AppUser polje postoji.  
     Joint table se koristi za Many-to-Many relationship (u ovom slucaju izmedju User-Stock). */

    [Table("Portfolios")]
    public class Portfolio // U principu, 1 Portfolio je 1 Stock za zeljenog AppUser-a
    {   
        // U ApplicationDbContext OnModelCreating definisacu PK kao kombinaciju AppUserId i StockId
        public string AppUserId { get; set; } // FK za AppUser koji automatski gadjda AppUser.Id PK ali moram definisati vezu u OnModelCreating. AppUserId je string, jer AppUser.Id tj IdentityUser.Id je GUID(String).
        public int StockId { get; set; }   // FK za Stock koji automatski gadja Stock.Id ali moram definisati vezu u OnModelCreating
        // AppUserId i StockId su zajedno PK jer Portfolio ima i AppUser i Stock 
        public AppUser AppUser { get; set; } // AppUser can have many portfolios (AppUser ima List<Portfolio> polje) ali moram definisati relaciju u OnModelCreating
        public Stock Stock { get; set; }   // Stock can have many Portfolios (Stock ima List<Portfolio> polje) ali moram definisati relaciju u OnModelCreating
        
        // Stock i AppUser su Circular reference, ali samo u Program to ugasio da ne izbaca gresku (pomocu JSON Serializer) jer mi treba bas ovako.
    }
    // Kad namestim sve za Portfolio -> Package Manager Console -> Migration da se pojavi ova tabela u bazi i onda rucno unesem kroz SQL Management jedan Portfolio na osnovu postojecih AppUsers i Stock
}
