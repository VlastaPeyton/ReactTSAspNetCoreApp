using Api.DTOs.Stock;
using Api.Helpers;
using Api.Models;

namespace Api.Interfaces
{   
    /* Repository pattern kako bi, umesto u StockController, u StockRepository definisali tela Endpoint metoda tj DB calls smestili u Repository.
       Koriscenje IStockRepository omogucava testiranje bez koriscenja baze + loose coupling.
    
       Ne korisitm StockDTO, vec Stock, jer Repository direkt sa bazom komunicira. */
    public interface IStockRepository
    {
        // Task, jer u StockRepository bice definisane kao async + ce u StockController Endpoint mozda da ih poziva pomocu await
        Task<List<Stock>> GetAllAsync(QueryObject query); 
        Task<Stock?> GetByIdAsync(int id); // FirstOrDefaultAsync can be NULL i zato Stock?
        Task<Stock> CreateAsync(Stock stock); 
        Task<Stock?> UpdateAsync(int id, Stock stock); // FindAsync can be NULL i zato Stock?
        Task<Stock?> DeleteAsync(int id); // FirstOrDefaultAsync can be NULL i zato Stock?
        Task<bool> StockExists(int id);
        Task<Stock?> GetBySymbolAsync(string symbol);
    }
}
