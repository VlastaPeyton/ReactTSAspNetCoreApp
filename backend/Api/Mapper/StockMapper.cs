using Api.DTOs.Stock;
using Api.DTOs.StockDTO;
using Api.DTOs.StockDTOs;
using Api.Models;

namespace Api.Mapper
{   
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
                // Ne mapiram Comments, jer je Navigation Attribute, pa je pomocu FK-PK povezan sa odgovorajucim vrstama iz Comments tabele
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
                // Ne mapiram Comments, jer taj atribut nema u UpdateStockRequestDTO posto je on Navigation Attribute u Stock i onda je povezan, pomocu PK-FK sa zeljenim Comments redovima
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
                // Comments i Portfolios are default values jer FinancialModelingPrepStockDTO nema ta polja niti nam trebaju jer to ubacamo mi kroz frontend 
            };
        }
    }
}
