namespace Api.Value_Objects
{
    // Strongly-typed Id. Record, a ne class, jer nema potrebe za class.
    public record CommentId
    {   
        public int Value { get; } // Nije Guid, jer app radi kada Id of Comment is int. Pa da ne moram brisati sve podatke i migrirati opet sve.
        private CommentId(int value) => Value = value; // Konstruktor na moderan nacin jer imam 1 polje samo
        
        // Of umesto public construktora je Ric domain model
        public static CommentId Of(int value) // Za sad je nepotreba jer u OnModelCreating automatski generisem Id polje Comments tabele
        {
            ArgumentNullException.ThrowIfNull(value); // Ovo je Domain layer i mora validacija u njemu
            return new CommentId(value);
        }
    }
}
