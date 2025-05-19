using System.ComponentModel.DataAnnotations.Schema;
using Api.DTOs.CommentDTOs;

namespace Api.DTOs.StockDTO
{
    /* StockDTO sluzi za Read/Write to DB from Endpointsjer zelim da razdvojim sloj koji pristupa bazi od API sloja kome pristupa korisnik tj ne zelim da 
    nikad direktno pristupam Stock.cs klasi jer je to definicija tabele u bazi. Takodje, DTO sluzi za Data Validation, jer se to ne radi u Entity klasi. 
    Bas zato, u svakom Endpoint koristim StockDTO, a ne Stock.

       Ne sadrzi Data Annotations kao Stock.cs, jer ovo nije definicija tabele. 

      StockDTO nema Data Validation, jer se to odigra u Create/UpdateStockRequestDTO, dok StockDTO sluzi samo za prikaz svega iz Stock.
    */
    public class StockDTO
    {
        public int Id { get; set; } // PK
        public string Symbol { get; set; } = string.Empty; // Ako ne unesem nista, u koloni Symbol bice prazan string 
        public string CompanyName { get; set; } = string.Empty;
        public decimal Purchase { get; set; }
        public decimal Dividend { get; set; }
        public string Industry { get; set; } = string.Empty;
        public long MarketCap { get; set; }

        public List<CommentDTO> Comments { get; set; } // U StockDTO koristim CommentDTO, ne Comment
    }
}
