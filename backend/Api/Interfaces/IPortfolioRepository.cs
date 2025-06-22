using Api.Models;

namespace Api.Interfaces
{   /* Repository pattern kako bi, umesto u PortfolioController, u PortfolioRepository definisali tela Endpoint metoda tj DB calls smestili u Repository.
       Koriscenje IPortfolioRepository omogucava testiranje bez koriscenja baze + loose coupling.
    
       Ne korisitm DTO klase, vec Models klase, jer Repository direkt sa bazom komunicira. */
    public interface IPortfolioRepository
    {
        Task<List<Stock>> GetUserPortfoliosAsync(AppUser user);
        Task<Portfolio> CreateAsync(Portfolio portfolio);

        Task<Portfolio> DeletePortfolio(AppUser appUser, string symbol);
    }
}
