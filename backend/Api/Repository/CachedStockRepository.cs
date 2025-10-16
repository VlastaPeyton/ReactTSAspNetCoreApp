using System.Text.Json;
using Api.Data;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Api.Repository
{   
    // Pogledaj Redis, Proxy & Decorator patterns.txt 
    public class CachedStockRepository : IStockRepository
    {
        private readonly IStockRepository _stockRepository;
        // CachedStockRepository dodaje cache logiku on top of StockRepository with Decorator pattern, pa u StockControler poziva se CachedStockRepository => moram koristim StockRepository tj IStockRepository jer u Program.cs IStockRepository registrovan kao StockRepository
        private readonly IDistributedCache _cache;
        // Program.cs with AddStackExchangeRedisCache konektujem Redis in Docker with IDistributedCache
        
        public CachedStockRepository(IStockRepository stockRepository, IDistributedCache cache)
        {
            _stockRepository = stockRepository;    
            _cache = cache;  
        }

        // Key u Redis mora biti jedinstven, pa mogu birati izmedju Id i Symbol polja u Stock klasi => Symbol je uniquem, ali biram Id jer on je sto posto jedinstven, ali moram ga prebaciti u $"stock:{stock.Id}" jer IDistributedCache radi samo sa string key/value
        
        public async Task<Stock> CreateAsync(Stock stock, CancellationToken cancellationToken)
        {   // Write-through strategija sa rucnom unosom u bazu a da Redis unosi automatski 

            // Prvo, upisem u bazu pomocu StockRepository metode
            await _stockRepository.CreateAsync(stock, cancellationToken); // Ne treba mi povratna vrednost ove metode i zato nisam naveo

            // Drugo, upisem i u Redis key-value, gde je value tipa string(JSON) jer samo ova metoda je built-in preko IDistributedCache
            await _cache.SetStringAsync($"stock:{stock.Id}", JsonSerializer.Serialize(stock), cancellationToken);

            return stock;
        }

        public async Task<Stock?> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            // Prvo, delete from DB
            var stock = await _stockRepository.DeleteAsync(id, cancellationToken);

            // Drugo, delete from Redis based on a key 
            await _cache.RemoveAsync($"stock:{id}", cancellationToken);

            return stock;
        }

        public async Task<List<Stock>> GetAllAsync(QueryObject query, CancellationToken cancellationToken)
        {
            // Nema potrebe cache ovde, jer GetAllAsync u StockRepository nema smisla za cache
            var listStocks = await _stockRepository.GetAllAsync(query, cancellationToken);
            return listStocks;
        }

        public async Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            // Cache-aside reading 

            // Prvo, proverim u cache
            var cachedStockJSON = await _cache.GetStringAsync($"stock:{id}", cancellationToken);
            // Ako ima u cache, onda vrati ga clientu
            if (!string.IsNullOrEmpty(cachedStockJSON))
                return JsonSerializer.Deserialize<Stock?>(cachedStockJSON)!; // Napravim Stock objekat iz JSON

            // Ako nema u cahce, uzmem iz baze, upisem u cache i vratim clientu
            var dbStock = await _stockRepository.GetByIdAsync(id, cancellationToken);
            await _cache.SetStringAsync($"stock:{id}", JsonSerializer.Serialize(dbStock), cancellationToken);
            return dbStock;
        }

        public async Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken)
        {
            // Necu da koristim Redis ovde, jer key je Stock.Id, a ne Stock.Symbol 
            var dbStock = await _stockRepository.GetBySymbolAsync(symbol, cancellationToken);
            return dbStock;
        }

        public async Task<bool> StockExists(int id, CancellationToken cancellationToken)
        {
            // Nema smisla koristiti cache ovde jer _stockRepository.StockExists ne vraca Stock objekat da bih ga upisao u cache, a da trazim u bazi, sporo je 
            bool stockExists = await _stockRepository.StockExists(id, cancellationToken);
            return stockExists;
        }

        public async Task<Stock?> UpdateAsync(int id, Stock stock, CancellationToken cancellationToken)
        {
            // Write-through za write 

            // Prvo, azuriram cache ako ga ima u cache ako ga ima 
            var cachedStockJSON = await _cache.GetStringAsync($"stock:{id}", cancellationToken);
            if (cachedStockJSON is not null)
            {
                var cachedStock = JsonSerializer.Deserialize<Stock?>(cachedStockJSON);
                // Azuriram samo polja koja su navedena u UpdateStockRequestDTO
                cachedStock.Symbol = stock.Symbol;
                cachedStock.CompanyName = stock.CompanyName;
                cachedStock.Purchase = stock.Purchase;
                cachedStock.Dividend = stock.Dividend;
                cachedStock.Industry = stock.Industry;
                cachedStock.MarketCap = stock.MarketCap;
            }
            // Drugo, ako nema u cache nema veze, onda samo bazu azuriram
            var dbStock = await _stockRepository.UpdateAsync(id, stock, cancellationToken);

            return dbStock;
        }
    }
}
