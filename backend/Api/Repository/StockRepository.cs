﻿using Api.Data;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository
{
    /* Repository pattern kako bi, umesto u StockController, u StockRepository definisali tela Endpoint metoda + DB calls u Repository smestim i zato
    ovde ne ide StockDTO, vec samo Stock jer Models klase su Entity tj za EF Core. 
         
       Repository interaguje sa bazom i ne zelim da imam DTO klase ovde, vec Entity klase i zato u Controller koristim mapper extensions da napravim Entity klasu from DTO klase.
      
       Objasnjenje za CancellationToken pogledaj u CommentController. 
    */
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDBContext _dbContext; 

        public StockRepository(ApplicationDBContext context) 
        {
            _dbContext = context;
        }

        // Sve metode su async, jer u StockController bice pozvace pomocu await. 
        // Metoda koja ima Stock?, zato sto compiler warning prikaze ako return moze biti null jer FirstOrDefault moze i null da vrati
        public async Task<Stock> CreateAsync(Stock stock, CancellationToken cancellationToken)
        {
            await _dbContext.Stocks.AddAsync(stock, cancellationToken); // EF starts tracking stock object i sve sto baza promeni u vrsti koja se odnosi na ovaj object, EF ce da promeni u stock object i obratno.
            // EF in Change Tracker marks stock tracking state to Added 
            await _dbContext.SaveChangesAsync(cancellationToken); // DB doda vrednost u Id kolonu nove vrste koja se odnosi na stock object => EF doda tu vrednost u Id polje of stock object zbog change tracking
            return stock; // isti stock, samo sa azuriranim Id poljem, jer EF does tracking
        }

        public async Task<Stock?> DeleteAsync(int id, CancellationToken cancellationToken)
        {   
            var stock = await _dbContext.Stocks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken); // EF tracks stock object, so every change made to stock will be applied to its corresponding row in Stocks table after SaveChangesAsync
            // As Id is PK for Stock, it is automatically also Index so this is fastest way for query
            if (stock is null)
                return null;

            _dbContext.Stocks.Remove(stock);     // EF in Change Tracker marks stock tracking state to Deleted 
            await _dbContext.SaveChangesAsync(cancellationToken); // stock is no longer tracked by EF

            return stock;
        }

        public async Task<List<Stock>> GetAllAsync(QueryObject query, CancellationToken cancellationToken)
        {   
            var stocks = _dbContext.Stocks.AsNoTracking().Include(c => c.Comments).ThenInclude(c => c.AppUser).AsQueryable(); // Dohvati sve stocks + njihove komentare + AppUser svakog komentara
            // Stock ima List<Comment> polje i FK-PK vezu sa Comment i zato moze include. Bez tog polja, moralo bi kompleksiniji LINQ.
            // AsQueryable zadrzava LINQ osobine, pa mogu kasnije npr stocks.Where(...)
            // Ovde nema EF change tracking jer AsNoTracking, obzirom da ne azuriram ono sto sam dohvatio, pa da neam bespotrebni overhead and memory zbog tracking

            // In if statement no need to AsQueryable again 
            if (!string.IsNullOrWhiteSpace(query.CompanyName))
                stocks = stocks.Where(s => s.CompanyName.Contains(query.CompanyName));

            if (!string.IsNullOrWhiteSpace(query.Symbol))
                stocks = stocks.Where(s => s.Symbol.Contains(query.Symbol));

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                if (query.SortBy.Equals("Symbol", StringComparison.OrdinalIgnoreCase))
                    stocks = query.IsDescending ? stocks.OrderByDescending(s => s.Symbol) : stocks.OrderBy(s => s.Symbol);
            
            var skipNumber = (query.PageNumber - 1) * query.PageSize; // Pagination

            return await stocks.Skip(skipNumber).Take(query.PageSize).ToListAsync(cancellationToken);
        }

        public async Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {  // Objasnjene za Include je u GetAllAsync
           // FirstOrDefaultAsync moze da vrati null ( i to bez if(stock is null)) i zato Stock? return type, da se compiler ne buni. 
           // FindAsync je brze od FirstOrDefaultAsync, ali nakon Include ne moze FindAsync.
            return await _dbContext.Stocks.AsNoTracking().Include(c => c.Comments).FirstOrDefaultAsync(i => i.Id == id, cancellationToken); // Dohvati zeljeni stock na osnovu Id polja + njegove komentare
           // EF track changes after FirstOrDefaultAsync ali mi to ovde ne treba i zato nema znak jednakosti + AsNoTracking ima jer tracking ubaca overhead and memory bespotrebno ovde
        }

        public async Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken)
        {   // FirstOrDefaultAsync moze da vrati null i zato Stock? return type, da se compiler ne buni. 
            return await _dbContext.Stocks.AsNoTracking().FirstOrDefaultAsync(s => s.Symbol == symbol, cancellationToken); // Ne moze FindAsync, iako je brze, jer FindAsync pretrazuje po Id samo
            // Iako ovaj Endpoit ne koristim cesto, jer retko pisem komentare za stock u FE, Stock.Symbol sam stavio u Index zbog DeletePortfolio, pa automatski i ovde brze ce da pretrazi
            // EF track changes after FirstOrDefaultAsync ali mi to ovde ne treba i zato nema znak jednakosti + AsNoTracking ima jer tracking ubaca overhead and memory bespotrebno ovde
        }

        public async Task<bool> StockExists(int id, CancellationToken cancellationToken)
        {
            return await _dbContext.Stocks.AnyAsync(s => s.Id == id, cancellationToken);
        }
       
        public async Task<Stock?> UpdateAsync(int id, Stock stock, CancellationToken cancellationToken)
        {   // FindAsync moze da vrati null i zato Stock? da compiler se ne buni 
            var existingStock = await _dbContext.Stocks.FindAsync(id, cancellationToken); // Brze nego FirstOrDefaultAsync, ali nema Include (jer ne trazim Comments/Portfolios pa mi ne treba), pa moze FindAsync 
            // EF will track existingStock after FindAsync, so any change made to existingStock will apply to DB after SaveChangesAsync
            // EF in Change Tracker marks existingStock tracking state to Unchanged jer ga je tek naso u bazi
            if (existingStock is null) // Mora da proverim, jer FindAsync vrati null ako nije nasla
                return null;

            // Azuriram samo polja koja su navedena u UpdateStockRequestDTO
            existingStock.Symbol = stock.Symbol;
            existingStock.CompanyName = stock.CompanyName;
            existingStock.Purchase = stock.Purchase;
            existingStock.Dividend = stock.Dividend;
            existingStock.Industry = stock.Industry;
            existingStock.MarketCap = stock.MarketCap;

            await _dbContext.SaveChangesAsync(cancellationToken); // Due to EF change tracking existingStock, this will apply changes in existingStock to corresponding row in DB Stocks table

            return existingStock;
        }
    }
}
