namespace Api.DTOs.CommentDTOs
{
    /* CommentDTO sluzi za Read/Write to DB from Endpointsjer zelim da razdvojim sloj koji pristupa bazi od API sloja kome pristupa korisnik tj ne zelim da 
    nikad direktno pristupam Comment.cs klasi jer je to definicija tabele u bazi. Takodje, DTO sluzi za Data Validation, jer se to ne radi u Entity klasi (u domain layeru) 
    Bas zato, u svakom Endpoint koristim StockDTO, a ne Stock.

       Ne sadrzi Data Annotations kao Comment.cs, jer ovo nije definicija tabele.
    
       Sve isto kao Comment, osim Stock polja koje je Navigation Property pa se ne moze dodati odavde nego preko PK-FK referencira zeljenu vrstu Stocks tabele. 
    
       CommentDTO nema Data Validation, jer se to odigrava u Create/UpdateCommentRequestDTO, dok CommentDTO sluzi samo za prikaz svega iz Comment. */
    public class CommentDTO
    {
        public int Id { get; set; } // PK
        public int? StockId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty; // Jer u Comment.cs imam AppUser polje 
    }
}
