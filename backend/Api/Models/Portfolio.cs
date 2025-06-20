using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    /*Join table koja ce da predstavlja each Stock of each AppUser (zato Stock polje postoji), jer nije dobra praksa da AppUser ima Stocks listu kao polje i Stock da ima AppUsers listu kao polje.
     Takodje, predstavlja AppUser of each Stock i zato AppUser polje postoji.  
     Joint table se koristi za Many-to-Many relationship (u ovom slucaju izmedju User-Stock). */

    [Table("Portfolios")]
    public class Portfolio // U principu, 1 Portfolio je 1 Stock za zeljenog AppUser-a
    {   
        // U ApplicationDbContext OnModelCreating definisacu PK kao kombinaciju AppUserId i StockId, jer to ovde ne moze.
        public string AppUserId { get; set; } // AppUserId je string, jer AppUser.Id tj IdentityUser.Id je GUID(String).
        public int StockId { get; set; }   // StockId je int, jer u Stock Id je int. 
        public AppUser AppUser { get; set; } // Ovo je 1-to-Many relationship jer Portfolio ima AppUser/Stock polje, pa EF zna da 1 AppUser/Stock moze imati/pripadati vise Portfolio
        public Stock Stock { get; set; } // Ovo je 1-to-Many relationship jer Portfolio ima AppUser/Stock polje, pa EF zna da 1 AppUser/Stock moze imati/pripadati vise Portfolio

        // Stock i AppUser su Circular reference, ali samo u Program to ugasio da ne izbaca gresku (pomocu JSON Serializer) jer mi treba bas ovako.
    }
    // Kad namestim sve za Portfolio -> Package Manager Console -> Migration da se pojavi ova tabela u bazi i onda rucno unesem kroz SQL Management jedan Portfolio na osnovu postojecih AppUsers i Stock
}
