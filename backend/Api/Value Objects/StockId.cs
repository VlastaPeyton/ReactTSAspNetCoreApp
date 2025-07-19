namespace Api.Value_Objects
{   // Strongly-typed Id. Record, a ne class, jer nema potrebe za class.
    public record StockId
    {
        public int Value { get; } // Nije Guid, jer app radi kada Id of Stock is int. Pa da ne moram brisati sve podatke i migrirati opet sve.
        private StockId(int value) => Value = value; // Konstruktor na moderan nacin jer imam 1 polje samo

        // Of umesto public construktora je Ric domain model
        public static StockId Of(int value) // Za sad mi ne treba ova metoda
        {
            ArgumentNullException.ThrowIfNull(value); // Ovo je Domain layer i mora validacija u njemu
            return new StockId(value);
        }
    }
}
