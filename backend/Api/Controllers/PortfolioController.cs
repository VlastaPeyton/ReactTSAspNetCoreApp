using Api.Extensions;
using Api.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/portfolio")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IStockRepository _stockRepository;
        private readonly IPortfolioRepository _portfolioRepository; // U Program registrovan IPortfolioRepository kao PortfolioRepository
        private readonly IFinacialModelingPrepService _finacialModelingPrepService;
        public PortfolioController(UserManager<AppUser> userManager, IStockRepository stockRepository, IPortfolioRepository portfolioRepository, IFinacialModelingPrepService finacialModelingPrepService)
        {
            _userManager = userManager;
            _stockRepository = stockRepository;
            _portfolioRepository = portfolioRepository;
            _finacialModelingPrepService = finacialModelingPrepService;
        }

        // Svaki Endpoint prima DTO klase kao argumente, jer to je dobra praksa da ne diram Models klase koje su za Repository namenjene.

        /* Svaki Endpoint bice tipa Task<IActionResult<T>> jer IActionResult<T> omoguci return of StatusCode + Data of type T, dok Task omogucava async. 
           
           Svaki Endpoint, salje to Frontend, Response koji ima polja Status Line, Headers i Body. 
        Status i Body najcesce definisem ja, a Header mogu ja u CreatedAtAction, ali najcesce to automatski .NET radi.
        Ako u objasnjenju return naredbe ne spomenem Header, to znaci da je on automatski popunjem podacima.
          
         Endpoint kad posalje Frontendu StatusCode!=2XX i mozda error data uz to, takav Response nece ostati u try block, vec ide u catch block i onda response=undefined u ReactTS.
        */

        [HttpGet]
        [Authorize] // U Swagger Authorize dugme moram uneti JWT from Login kako bih mogo da pokrenem ovaj Endpoint
        public async Task<IActionResult> GetUserPortfolios()
        {   
            // I da nisam stavio [Authorize], zbog User.GetUserName moral bi da se JWT prosledi sa Frontend prilikom gadjanja ovog Endpoint, ali treba staviti [Authorize] jer osigurava da ovo ne bude null
            var userName = User.GetUserName(); // User i GetUserName come from ControllerBase Claims  i odnose se na current logged user
            var appUser = await _userManager.FindByNameAsync(userName); // Built-in fora

            var userPortfolios = await _portfolioRepository.GetUserPortfoliosAsync(appUser);
            
            return Ok(userPortfolios);
            // Frontendu ce biti poslato StatusCode=200 u Response StatusLine, a userPortfolios u Response Body.
        }

        [HttpPost]
        [Authorize]
        /* .NET ce da potrazi symbol argument i u Body, Query i URL ako nije explicitno navedeno nista od [FromBody]/[FromRoute]/[FromQuery]...
        sto znaci da iz ReactTS Frontend mogu odakle god da posaljem ovaj argument. */
        public async Task<IActionResult> AddPortfolio(string symbol)  // 1 Portfolio = 1 Stock, a glavna stvar Stock-a je Symbol polje
        {   // I da nisam stavio [Authorize], zbog User.GetUserName() moralo bi da se JWT prosledi sa Frontend prilikom gadjanja ovog Endpoint, ali stavim [Authorize] jer osigura da userName!=null, jer forsira Frontend da posalje JWT.
            var userName = User.GetUserName(); // User i GetUserName come from ControllerBase Claims 
            var appUser = await _userManager.FindByNameAsync(userName);

            var stock = await _stockRepository.GetBySymbolAsync(symbol); // Obraca se bazi
            // Ako ne postoji Stock u bazi, trazi ga na netu
            if (stock is null)
            { 
                stock = await _finacialModelingPrepService.FindStockBySymbolAsync(symbol);
                if (stock is null)
                    return BadRequest("Nepostojeci stock symbol koji nema ni na netu");
                    // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a "Nepostojeci stock symbol koji nema ni na netu" u Response Body.
                else 
                    await _stockRepository.CreateAsync(stock);  
            }

            var userPortfolios = await _portfolioRepository.GetUserPortfoliosAsync(appUser); // Svi Stock koje appUser ima vec 
            if (userPortfolios.Any(e => e.Symbol.ToLower() == symbol.ToLower()))
                return BadRequest("Cannot add same stock to portfolio as this user has already this stock in portfolio");
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a "Cannot add same stock to portfolio as this user has already this stock in portfolio" u Response Body.

            var portfolio = new Portfolio // Jer Stock je jedan Portfolio
            {
                StockId = stock.Id,
                AppUserId = appUser.Id,
                Stock = stock,
                AppUser = appUser,
            };

            await _portfolioRepository.CreateAsync(portfolio);

            if (portfolio is null)
                return StatusCode(500, "Could not create Portfolio");
                // Frontendu ce biti poslato StatusCode=500 u Response Status Line, a "Could not create Portfolio" u Response Body.
            else
                return Created();
                // Frontendu ce biti poslato StatusCode=201 u Response Status Line, a Response Body prazan.
        }

        [HttpDelete]
        [Authorize]
        /* .NET ce da potrazi symbol argument i u Body, Query i URL ako nije explicitno navedeno nista od [FromBody]/[FromRoute]/[FromQuery]...
        sto znaci da iz ReactTS Frontend mogu odakle god da posaljem ovaj argument. */
        public async Task<IActionResult> DeletePortfolio(string symbol) // 1 Portfolio = 1 Stock, a glavna stvar Stock-a je Symbol polje
        {   // I da nisam stavio [Authorize], zbog User.GetUserName moral bi da se JWT prosledi sa Frontend prilikom gadjanja ovog Endpoint, ali treba staviti [Authorize] jer osigurava da userName!=null, jer forsira Frontend da salje JWT.
            var userName = User.GetUserName();
            var appUser = await _userManager.FindByNameAsync(userName);

            var portfolios = await _portfolioRepository.GetUserPortfoliosAsync(appUser); // Portfolio list of Stock elements 
            
            var stockInPortfolios = portfolios.Where(s => s.Symbol.ToLower() == symbol.ToLower()).ToList(); // Nema treba async, jer ne pretrazujem u bazu, vec in-memory varijablu

            if (stockInPortfolios.Count() == 1) // is not null jer samo 1 Stock unique stock moze biti u portfolios list, jer AddPortfolio metoda iznad to ogranicila
                await _portfolioRepository.DeletePortfolio(appUser, symbol);

            else
                return BadRequest("Stock is not in your portfolio list");
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a "Stock is not your portfolio list" u Response Body.

            return Ok();
            // Frontendu ce biti poslato StatusCode=200 u Response Status Line, a Response Body prazan.
        }
    }
}
