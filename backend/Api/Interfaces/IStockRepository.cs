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
        // Task, jer u StockRepository metode bice definisane kao async + ce u StockController Endpoint mozda da ih poziva pomocu await
        // Metoda koja ima Stock?, zato sto compiler warning prikaze ako method's return moze biti null jer FirstOrDefault/FindAsync moze i null da vrati 

        Task<List<Stock>> GetAllAsync(QueryObject query); 
        Task<Stock?> GetByIdAsync(int id); 
        Task<Stock> CreateAsync(Stock stock); 
        Task<Stock?> UpdateAsync(int id, Stock stock); 
        Task<Stock?> DeleteAsync(int id); 
        Task<bool> StockExists(int id);
        Task<Stock?> GetBySymbolAsync(string symbol); 
    }
}
