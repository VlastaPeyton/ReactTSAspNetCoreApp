using Api.Models;

namespace Api.Interfaces
{
    // Do sad smo sa ovog sajta ocitavali u ReactJS sve Stocks, ali sad cu da Seed DB with data from this site

    // Ovo nije repository, jer FinancialModleingPrepService je web API koji gadjam tj nije moj licni
    public interface IFinacialModelingPrepService
    {
        Task<Stock> FindStockBySymbolAsync(string symbol);// Stock se razaznaje by symbol npr : Microsoft je MSFT symbol
    }
}
