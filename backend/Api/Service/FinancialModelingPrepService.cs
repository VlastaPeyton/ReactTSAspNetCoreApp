using Api.DTOs.StockDTOs;
using Api.Interfaces;
using Api.Mapper;
using Api.Models;
using Newtonsoft.Json;

namespace Api.Service
{   
    // U FE, zelim da ostavim komentar za npr "TSLA" stock, kada ukucam "TSLA" u search, prover ima li ga u bazi, ako nema, onda ga skida sa FinancialModelingPrep, stavi u bazu, pa onda mu okacim komentar
    
    // CancellationToken objasnjen u bilo kojoj Controller klasi.
    public class FinancialModelingPrepService : IFinacialModelingPrepService
    {   
        private HttpClient _httpClient; // Za sljanje Request to web API. U Program.cs sam registrovao ovo za FinancialModelingPrepService 
        private IConfiguration _configuration; // Dohvata appsettings.json
        public FinancialModelingPrepService(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;  
            _httpClient = httpClient;     
        }

        public async Task<Stock?> FindStockBySymbolAsync(string symbol, CancellationToken cancellationToken) 
        // Stock?, a ne Stock, jer return null ima i onda da compiler ne kuka
        {   
            // Mora try-catch zog HttpClient
            try
            {
                var result = await _httpClient.GetAsync($"https://financialmodelingprep.com/api/v3/profile/{symbol}?apikey={_configuration["FMPApiKey"]}", cancellationToken);
                // result contains StatusCode, Header, Body ...
                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync(cancellationToken); // content = Response Body. Niz objekata jer tako ovaj sajt vraca 
                    var stocks = JsonConvert.DeserializeObject<FinancialModelingPrepStockDTO[]>(content);
                    // Convert JSON to list of FinancialModelingPrepStock objects, jer FinancialModelingPrep vraca niz tipa FinancialModelingPrepStockDTO, ali nam samo prvi element treba jer 1 element ocemo
                    var stock = stocks[0]; // jer nam samo prvi elem treba posto je to nas stock trazeni
                    if (stock is not null)
                        return stock.ToStockFromFinancialModelingPrepStockDTO();
                    else
                        return null;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
