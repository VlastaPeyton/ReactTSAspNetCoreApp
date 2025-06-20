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
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; } = DateTime.Now; 

        public string AppUserId { get; set; } // AppUserId je string, jer AppUser.Id je GUID(String).
        public AppUser AppUser { get; set; }

        /* AppUser i Stock polje nece postojati kao kolona u tabeli, jer je Reference type + AppUser nije Value Object (jer AppUser ima Id polje) i nije [Owned] tj ovo je Navigational property da mozemo u LINQ za Comment dohvatiti AppUser pomocu Include.
         Zbog AppUser i Stock polja, u Program.cs dodajem sta treba da sprecim circular reference jer ovo je Circular reference koji je nepozeljan ali mi treba ovako jer ne znam bolje. 

          Ovo je 1-to-Many relationship jer Comment ima AppUser polje pa EF zna da 1 AppUser moze imati Many Comments.
          Ovo je 1-to-Many relatiosnhip je Comment ima Stock polje pa EF zna da 1 Stock moze imati Many Comments.
        
         Mora Migration da se odradi nakon dodavanja ovog polja, ali pre toga izbrisati sve iz Comments tabele jer inace greska, pa je onda napunim.
         */
    }
}
