using Api.DTOs.Stock;
using Api.DTOs.StockDTO;
using Api.DTOs.StockDTOs;
using Api.Models;

namespace Api.Mapper
{
    // Da napravim Stock Entity klasu od DTO klasa kada pokrecem Repository metode.
    // Da napravim DTO klasu od Stock Entity klasa kada saljem data to FE in Endpoint
    public static class StockMapper
    {   
        // Extension method for Stock.cs, without calling parameters
        public static StockDTO ToStockDTO(this Stock stock)
        {
            return new StockDTO
            {
                Id = stock.Id, // Id mapiram, jer mapiram iz Stock to DTO
                Symbol = stock.Symbol,
                CompanyName = stock.CompanyName,
                Purchase = stock.Purchase,
                Dividend = stock.Dividend,
                Industry = stock.Industry,
                MarketCap = stock.MarketCap,
                Comments = stock.Comments.Select(c => c.ToCommentDTO()).ToList() // Ovako mapiram listu
            };
        }

        // Extension method for CreateStockRequestDTO.cs, without calling parameters
        public static Stock ToStockFromCreateStockRequestDTO(this CreateStockRequestDTO createStockRequestDTO)
        {
            return new Stock
            {   // Ne mapiram Id iz DTO, jer se prilikom dodavanja u bazu sam generise 
                Symbol = createStockRequestDTO.Symbol,
                CompanyName = createStockRequestDTO.CompanyName,
                Purchase = createStockRequestDTO.Purchase,
                Dividend = createStockRequestDTO.Dividend,
                Industry = createStockRequestDTO.Industry,
                MarketCap = createStockRequestDTO.MarketCap,
                /* Ne mapiram Comments/Portfolios polja, jer nisu prisutna u CreateStockRequestDTO posto su collection navigation attribute u Stock pa imaju default polje, a nije ni logicno da postoje u UpdateStockRequestDTO
                Ova polja, kao i FK PK polja u Stock/Comment/Portfolio sluze da EF (ili ja u OnModelCreating), moze da napravim FK-PK vezu Stock-Comment/Portfolio i zato se ne salju from FE. */
            };
        }

        // Extension method for UpdateStockRequestDTO, without calling parameters
        public static Stock ToStockFromUpdateStockRequestDTO(this UpdateStockRequestDTO updateStockRequestDTO)
        {
            return new Stock
            {
                // Ne mapiram Id iz DTO, jer se prilikom dodavanja u bazu sam generise 
                Symbol = updateStockRequestDTO.Symbol,
                CompanyName = updateStockRequestDTO.CompanyName,
                Purchase = updateStockRequestDTO.Purchase,
                Dividend = updateStockRequestDTO.Dividend,
                Industry = updateStockRequestDTO.Industry,
                MarketCap = updateStockRequestDTO.MarketCap,
                /* Ne mapiram Comments/Portfolios polja, jer nisu prisutna u UpdateStockRequestDTO posto su collection navigation attribute u Stock pa imaju default polje, a nije ni logicno da postoje u UpdateStockRequestDTO
                 Ova polja, kao i FK PK polja u Stock/Comment/Portfolio sluze da EF (ili ja u OnModelCreating), moze da napravim FK-PK vezu Stock-Comment/Portfolio i zato se ne salje from FE.  */

            };
        }

        // Extension method for FinancialModelingPrepStockDTO, without calling parameters
        public static Stock ToStockFromFinancialModelingPrepStockDTO(this FinancialModelingPrepStockDTO financialModelingPrepStockDTO)
        {
            return new Stock
            {
                Symbol = financialModelingPrepStockDTO.symbol,
                CompanyName = financialModelingPrepStockDTO.companyName,
                Purchase = financialModelingPrepStockDTO.price,
                Dividend = financialModelingPrepStockDTO.lastDiv,
                Industry = financialModelingPrepStockDTO.industry,
                MarketCap = financialModelingPrepStockDTO.mktCap
                /* Ne mapiram Comments/Portfolios polja, jer nisu prisutna u FinancialModelingPrepStockDTO posto su collection navigation attribute u Stock pa imaju default polje, a nije ni logicno da postoje u UpdateStockRequestDTO
                 Ova polja, kao i FK PK polja u Stock/Comment/Portfolio sluze da EF (ili ja u OnModelCreating), moze da napravim FK-PK vezu Stock-Comment/Portfolio i zato se ne salju from FE. */
            };
        }
    }
}
