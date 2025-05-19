using Api.DTOs.StockDTOs;
using Api.Interfaces;
using Api.Mapper;
using Api.Models;
using Newtonsoft.Json;

namespace Api.Service
{   
    // Prilikom dodavanja Stock u portfolio, ako Stock ne postoji u bazi, skini ga sa neta, stavi u bazu, pa dodaj u portfolio iz baze.
    public class FinancialModelingPrepService : IFinacialModelingPrepService
    {   
        private HttpClient _httpClient; // Za citanje sa neta. U Program.cs sam registrovao ovo za FinancialModelingPrepService 
        private IConfiguration _configuration; // Dohvata appsettings.json
        public FinancialModelingPrepService(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        // Prilikom dodavanja Stock u portfolio, ako Stock ne postoji u bazi, skini ga sa neta, stavi u bazu, pa dodaj u portfolio iz baze.
        public async Task<Stock> FindStockBySymbolAsync(string symbol)
        {   
            // Mora try-catch, jer idem u Financial Modeling Prep website da uzmem podatke
            try
            {
                var result = await _httpClient.GetAsync($"https://financialmodelingprep.com/api/v3/profile/{symbol}?apikey={_configuration["FMPApiKey"]}");
                // result contains StatusCode, Headers, Payload (Content)...
                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync(); // Niz objekata jer tako ovaj sajt vraca 
                    var stocks = JsonConvert.DeserializeObject<FinancialModelingPrepStockDTO[]>(content);
                    // Convert JSON to list of FinancialModelingPrepStock objects, jer FinancialModelingPrep vraca u obliku niza, ali nam samo prvi element treba 
                    // Ovo sam imao u Frontend delu ista fora da FinancialModelingPrepStock ima polja kao objekat from result. 
                    var stock = stocks[0]; // jer nam samo prvi elem treba 
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
            // Iako nije Stock?, moze da vrati null, ali imacu warning samo 
        }
    }
}
