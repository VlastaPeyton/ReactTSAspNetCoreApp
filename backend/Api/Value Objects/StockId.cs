namespace Api.Value_Objects
{   // Strongly-typed Id. Record, a ne class, jer nema potrebe za class.
    public record StockId
    {
        public int Value { get; } // Nije Guid, jer app radi kada Id of Stock is int. Pa da ne moram brisati sve podatke i migrirati opet sve.
        private StockId(int value) => Value = value; // Konstruktor na moderan nacin jer imam 1 polje samo

        public static StockId Of(int value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return new StockId(value);
        }
    }
}
