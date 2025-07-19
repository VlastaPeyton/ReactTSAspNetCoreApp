using System.ComponentModel.DataAnnotations.Schema;
using Api.Value_Objects;

namespace Api.Models
{
    // Models folder sluzi za Entity klase jer te klase ce biti tabele u bazi. 

    // Comment table i ovo se koristi samo za EF tj bazu. Za Endpoints se koristi CommentDTO
    [Table("Comments")]
    public class Comment // Entity jer predstavlja tabelu u bazi
    {
        public CommentId Id { get; set; } // PK and Index by default. Bio je int, ali sam stavio custom type.
        public int StockId { get; set; } // FK koji gadja Id u Stock klasi mora biti istog tipa kao Id u Stock klasi.
        public Stock? Stock { get; set; } // Navigational property
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.Now; 
        public string AppUserId { get; set; } // AppUserId je string, jer AppUser.Id je String. Ovo je FK koji gadja Id u AppUser klasi
        public AppUser AppUser { get; set; } // Navigational property 

        // Navigational property objasnjeje u AppUser ! 

        /*
          Ovo je 1-to-Many AppUser-Comment relationship jer Comment ima AppUser i AppUserId polje, dok AppUser ima List<Comment> polje, pa EF zakljuci ovu vezu na osnovu imena polja bez da moram pisati u OnModelCreating.
          Ovo je 1-to-Many Stock-Comment relatiosnhip je Comment ima Stock i StockId polje, dok Stocl ima List<Comment> polje, pa EF zakljuci ovu vezu na osnovu imena polja bez da moram pisati u OnModelCreating.
        
          Mora Migration da se odradi nakon dodavanja ovog polja, ali pre toga izbrisati sve iz Comments tabele jer inace greska, pa je onda napunim.
         */
    }
}
