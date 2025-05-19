using Api.Data;
using Api.DTOs.Stock;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository
{
    /* Repository pattern kako bi, umesto u StockController, u StockRepository definisali tela Endpoint metoda + DB calls u Repository se stavljaju i zato
    ovde ne ide StockDTO, vec samo Stock. 
      Moze u UpdateAsync, umesto Stock da bude UpdateStockRequestDTO, ali sam u StockMapper napravio Extension Method "ToStockFromUpdateStockRequestDTO", jer 
    Repository interaguje sa bazom i ne zelim da imam DTO klase ovde. */
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDBContext _dbContext; 
        public StockRepository(ApplicationDBContext context) 
        {
            _dbContext = context;
        }

        public async Task<Stock> CreateAsync(Stock stock)
        {
            await _dbContext.Stocks.AddAsync(stock);
            await _dbContext.SaveChangesAsync();
            return stock;
        }

        public async Task<Stock?> DeleteAsync(int id)
        {
            var stock = await _dbContext.Stocks.FirstOrDefaultAsync(x => x.Id == id);
            if (stock is null)
                return null;

            _dbContext.Stocks.Remove(stock);
            await _dbContext.SaveChangesAsync();

            return stock;
        }

        public async Task<List<Stock>> GetAllAsync(QueryObject query)
        {   /* U StockController, ova metoda bice pozvana pomocu await i zato je ovde Task. 
               Incude koristim, jer zelim sve Comments za svaki Stock da dohvatim iz baze, gde Comment FK gadja PK of Stock jer je Navigational Attribute. 
             Bez Include, ocitace sva polja tabele Stocks, bez Comments Navigational Attribute jer se on ocitava samo pomocu Include. */
            var stocks = _dbContext.Stocks.Include(c => c.Comments).ThenInclude(c => c.AppUser).AsQueryable();
            
            // Pogledaj QueryObject i bice jasno 
            if (!string.IsNullOrWhiteSpace(query.CompanyName))
                stocks = stocks.Where(s => s.CompanyName.Contains(query.CompanyName));

            if (!string.IsNullOrWhiteSpace(query.Symbol))
                stocks = stocks.Where(s => s.Symbol.Contains(query.Symbol));

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                if (query.SortBy.Equals("Symbol", StringComparison.OrdinalIgnoreCase))
                    stocks = query.IsDescending ? stocks.OrderByDescending(s => s.Symbol) : stocks.OrderBy(s => s.Symbol);
            
            var skipNumber = (query.PageNumber - 1) * query.PageSize; // Pagination

            return await stocks.Skip(skipNumber).Take(query.PageSize).ToListAsync();
        }

        public async Task<Stock?> GetByIdAsync(int id)
        {   /* Include moze jer Comment ima FK koji gadja PK u Stock. 
               Incude koristim, jer zelim sve Comments za svaki Stock da dohvatim iz baze, gde Comment FK gadja PK of Stock jer je Navigational Attribute. 
             Bez Include, ocitace sva polja tabele Stocks, bez Comments Navigational Attribute jer se on ocitava samo pomocu Include.
            
               FindAsync je brze od FirstOrDefaultAsync, ali nakon Include ne moze FindAsync. */
            return await _dbContext.Stocks.Include(c => c.Comments).FirstOrDefaultAsync(i => i.Id == id); 
        }

        public async Task<Stock?> GetBySymbolAsync(string symbol)
        {
            return await _dbContext.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol); // Ne moze FindAsync, iako je brze, jer FindAsync pretrazuje po Id 
        }

        public async Task<bool> StockExists(int id)
        {
            return await _dbContext.Stocks.AnyAsync(s => s.Id == id);
        }

        public async Task<Stock?> UpdateAsync(int id, Stock stock)
        {
            var existingStock = await _dbContext.Stocks.FindAsync(id); // Brze nego FirstOrDefaultAsync, ali nema Include jer ne trazim Comments, pa moze FindAsync 
            if (existingStock is null)
                return null;

            // Azuriram samo polja koja su navedena u UpdateStockRequestDTO
            existingStock.Symbol = stock.Symbol;
            existingStock.CompanyName = stock.CompanyName;
            existingStock.Purchase = stock.Purchase;
            existingStock.Dividend = stock.Dividend;
            existingStock.Industry = stock.Industry;
            existingStock.MarketCap = stock.MarketCap;

            await _dbContext.SaveChangesAsync();

            return existingStock;
        }
    }
}
