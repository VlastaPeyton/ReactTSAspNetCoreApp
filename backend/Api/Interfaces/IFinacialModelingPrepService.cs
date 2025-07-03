using Api.Models;

namespace Api.Interfaces
{
    // Za svaku klasu koja predstavlja Service pravim interface pomocu koga radim DI u Controller, dok u Program.cs pisem da prepozna interface kao zeljenu klasu

    // Do sad smo sa ovog sajta ocitavali u ReactJS sve Stocks, ali sad cu da Seed DB with data from this site

    // Ovo nije repository, jer FinancialModleingPrepService je web API koji gadjam tj nije moj licni

    // CancellationToken objasnjen u bilo kojoj Controller klasi
    public interface IFinacialModelingPrepService
    {
        Task<Stock> FindStockBySymbolAsync(string symbol, CancellationToken cancellationToken);// Stock se razaznaje by symbol npr : Microsoft je MSFT symbol
    }
}
