using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    // Comment table i ovo se koristi samo za EF tj bazu. Za Endpoints se koristi CommentDTO
    [Table("Comments")]
    public class Comment // Entity jer predstavlja tabelu u bazi
    {
        public int Id { get; set; } // PK
        
        public int? StockId { get; set; } // FK koji gadja Id u Stock klasi
        public Stock? Stock { get; set; } 
        /* Ovo nece postojati kao kolona u tabeli, jer je Reference type + Stock nije Value Object (jer Stock ima Id polje) i nije [Owned]. 
         Jedino preko FK i PK jer StockId gadjace Id u Stock klasi (OnModelCreating namesteno to + EF zna da spoji na osnovu imena), a onda su to dve tabele. 
         Zbog ovoga i Comments polja u Stock, u Program.cs dodajem sta treba da sprecim circular reference jer ovo je Circular reference. */

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; } = DateTime.Now; 

        public string AppUserId { get; set; } // AppUserId je string, jer AppUser.Id je GUID(String).
        public AppUser AppUser { get; set; }
        /* Ovo nece postojati kao kolona u tabeli, jer je Reference type + AppUser nije Value Object (jer AppUser ima Id polje) i nije [Owned]. 
         Jedino preko FK i PK, jer AppUserId gadjace Id u AppUser klasi ((OnModelCreating namesteno to + EF zna da spoji na osnovu imena), a onda su to dve tabele.
        Mora Migration da se odradi nakon dodavanja ovog polja, ali pre toga izbrisati sve iz Comments tabele jer inace greska, pa je onda napunim.
          Ovo je One-to-One relationship for AppUser-Comment jer kad user ostavi commentar, da se vidi njegovo ime.
          U CommentRepository/StockRepository ako zelim da dohvatim ovo polje, moram koristiti Include. */
    }
}
