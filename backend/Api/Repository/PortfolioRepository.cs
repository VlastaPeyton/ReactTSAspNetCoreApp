using Api.Data;
using Api.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository
{
    public class PortfolioRepository : IPortfolioRepository
    {   
        private readonly ApplicationDBContext _dbContext;
        public PortfolioRepository(ApplicationDBContext context) 
        {
            _dbContext = context;
        }

        public async Task<Portfolio> CreateAsync(Portfolio portfolio)
        {
            await _dbContext.Portfolios.AddAsync(portfolio);
            await _dbContext.SaveChangesAsync();
            return portfolio;
        }

        public async Task<Portfolio> DeletePortfolio(AppUser appUser, string symbol)
        {
            var portfolio = await _dbContext.Portfolios.FirstOrDefaultAsync(p => p.AppUserId == appUser.Id && p.Stock.Symbol.ToLower() == symbol.ToLower());
            if (portfolio == null)
                return null;

            _dbContext.Portfolios.Remove(portfolio);
            await _dbContext.SaveChangesAsync();

            return portfolio;
             
        }

        public async Task<List<Stock>> GetUserPortfoliosAsync(AppUser user)
        {   // Objasnjeno u OnModelCreating veza izmedju Portfolio-AppUser/Stock 
            return await _dbContext.Portfolios.Where(u => u.AppUserId == user.Id)
                                        // Za svaki Portfolio kreira Stock na osnovu podatka iz Portfolio, jer 1 Stock je 1 Portfolio 
                                        .Select(portfolio => new Stock
                                        {
                                        Id = portfolio.StockId, // StockId polje iz Portfolio
                                        Symbol = portfolio.Stock.Symbol, // Stock polje iz Portfolio, a Stock klasa ima Symbol polje
                                        CompanyName = portfolio.Stock.CompanyName,
                                        Purchase = portfolio.Stock.Purchase,
                                        Dividend = portfolio.Stock.Dividend,
                                        Industry = portfolio.Stock.Industry,
                                        MarketCap = portfolio.Stock.MarketCap
                                        // Ne prosledjujem Comments and Portfolios polja, jer portfolio ih nema + imaju defaul vrednost u Stock
                                        }).ToListAsync();
        }
    }
}