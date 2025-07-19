namespace Api.Value_Objects
{   
    // Strongly-typed Id
    public class CommentId
    {   
        public int Value { get; } // Nije Guid, jer app radi kada Id of Comment is int. Pa da ne moram brisati sve podatke i migrirati opet sve.
        private CommentId(int value) => Value = value; // Konstruktor na moderan nacin jer imam 1 polje samo

        public static CommentId Of(int value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return new CommentId(value);
        }
    }
}
