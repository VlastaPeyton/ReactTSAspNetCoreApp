using Api.Extensions;
using Api.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers
{
    [Route("api/portfolio")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {    // Interface za sve klase zbog DI, dok u Program.cs napisem da prepozna interface kao tu klasu
        private readonly UserManager<AppUser> _userManager; // Moze jer AppUser:IdentityUser
        private readonly IStockRepository _stockRepository;
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IFinacialModelingPrepService _finacialModelingPrepService;
        public PortfolioController(UserManager<AppUser> userManager, IStockRepository stockRepository, IPortfolioRepository portfolioRepository, IFinacialModelingPrepService finacialModelingPrepService, ILogger<PortfolioController> logger)
        {   // U Program.cs registrovan IStockRepository/IPortfolioRepository/IFinancialModelingPreprService kao StockRepository/PortfolioRepository/FinancialModelingPreprService 
            _userManager = userManager;
            _stockRepository = stockRepository;
            _portfolioRepository = portfolioRepository;
            _finacialModelingPrepService = finacialModelingPrepService;
        }

        /* 
           Za svaki Request pravi se automatski nova instanca kontrolera (AddTransient fakticki), onda DI automatski, na osnovu AddTransient/Scoped/Singleton<IService,Service>() iz Program.cs, u ctor kontrolera doda zeljeni servis i kad se 
          response posalje u FE, GC unistava kontroler, ali zivot servisa zavisi da li je Singleton, Transient ili Scoped.

         HttpContext je objekat koji nosi info o Request, Response, logged in User, Session itd. ControllerBase pruza polja vezana za HttpContext kao sto je User(HttpContext.User) koji sadrzi sve user info from request (stateless) - pogledaj Authentication middleware.txt

         Svaki Endpoint:
            - koristi DTO kao argumente i DTO za slanje objekata to FE, jer dobra praksa je ne dirati Models (Entity) klase (koje predstavljaju tabele u bazi) koje su namenjene za Repository tj EF Core.
            - bice tipa Task<IActionResult<T>> jer IActionResult<T> omoguci return of StatusCode + Data of type T, dok Task omogucava async. 
            - salje to FE Response koji ima polja Status Line, Headers i Body. 
              Status i Body najcesce definisem ja, a Header mogu ja u CreatedAtAction, ali najcesce to automatski .NET radi.
              Ako u objasnjenju return naredbe ne spomenem Header, to znaci da je on automatski popunjem podacima.
              Endpoint kad posalje Frontendu StatusCode!=2XX i mozda error data uz to, takav Response nece ostati u FE try block, vec ide u catch block i onda response=undefined u ReactTS.
         
         Data validation when writing to DB: 
            Request object je Endpoint argument koji stize from FE in order to write/read DB. Request object is never Entity class, but DTO class as i want to split api from domain/infrastructure layer. 
            If Endpoint exhibits write to DB, i have to validate Request DTO object fields before it is mapped to Entity class and written to DB. 
            Validation can be done:
                1) using ModelState - default validation logic. U write to DB endpoint, stavim ModelState koji zna da treba da validira annotated polja iz Request DTO object tog endpointa.
                2) using FluentValidation - ako zelim custom validation logic. 
         
         Ne koristim FluentValidation jer za sad nema potrebe, a samo ce da mi napravi more complex code. Koristim ModelState. 

         Ako endpoint nema [Authorize], FE ne treba slati JWT in Request Header.
            
         Nisam koristio cancellationToken = default, jer ako ReactTS pozove  Endpoint, i user navigates away or closes app, .NET ce automtaski da shvati da treba prekinuti izvrsenje i dodelice odgovarajucu vrednost tokenu. 
         Zbog nemanja =defaul ovde, ne smem imati ni u await metodama koje se pozivaju u Endpointu.
         
         Controller radi mapiranje entity klasa u DTO osim ako koristim CQRS, jer nije dobro da repository vrati DTO obzriom da on radi sa domain i treba samo za entity klase da zna

         Za async Endpoints, nisam koristio cancellationToken = default, jer ako ReactTS pozove ovaj Endpoint, i user navigates away or closes app, .NET ce automtaski da shvati da treba prekinuti izvrsenje i dodelice odgovarajucu vrednost tokenu. 
        Zbog nemanja =default ovde, ne smem imati ni u await metodama koje se pozivaju u Endpointu. 
        Da sam koristio =default ovde, .NET ne bi znao da automatski prekine izvrsenje Endpointa, pa bih morao u FE axios metodi da prosledim i controller.signal...
        CT se stavlja za time-consuming await metode npr duga ocitavanja u bazi, ali ja cu staviti na sve, zlu ne trebalo.
         
         Rate Limiter objasnjen u Program.cs
         */

        [HttpGet]
        [Authorize] // U Swagger Authorize dugme moram uneti JWT from Login kako bih mogo da pokrenem ovaj Endpoint
        public async Task<IActionResult> GetUserPortfolios(CancellationToken cancellationToken)
        {   
            var userName = User.GetUserName(); // User i GetUserName come from ControllerBase ClaimsPrincipal i odnose se na current logged user jer mnogo je lakse uzeti UserName/Email iz request nego iz baze
            var appUser = await _userManager.FindByNameAsync(userName); // Pretrazuje AspNetUsers tabelu da nadje AppUser 
            // userManager metode nemaju cancellationToken 

            var userPortfolios = await _portfolioRepository.GetUserPortfoliosAsync(appUser, cancellationToken);
            
            return Ok(userPortfolios);
            // Frontendu ce biti poslato StatusCode=200 u Response StatusLine, a userPortfolios u Response Body.
        }

        //[EnableRateLimiting("fast")] // Jer cesto koristim ovu metodu
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPortfolio([FromQuery] string symbol, CancellationToken cancellationToken)  // 1 Portfolio = 1 Stock, a glavna stvar Stock-a je Symbol polje
        {   // Da nema [FromQuery], obzirom da symbol je string, .NET bi prihvatamo i [FromRoute], [FromQuery] i [FromBody]. Zbog [FromQuery] u portfolioAddApi u FE moram poslati symbol nakon ? in URL
            var userName = User.GetUserName(); // User i GetUserName come from ControllerBase ClaimsPrincipal tj user info from request 
            var appUser = await _userManager.FindByNameAsync(userName); // Ne moze cancellationToken ovde

            // Kada u search ukucan npr "tsla" izadje mi 1 ili lista Stocks ili ETFs koji pocinju sa "tsla" i zelim kliknuti Add da dodam bilo koji stock u portfolio, pa prvo provera da li je zeljeni stock u bazi
            var stock = await _stockRepository.GetBySymbolAsync(symbol, cancellationToken); 
            // ako ne postoji Stock u bazi, trazi ga na netu u FininacinalModelingPrep
            if (stock is null)
            {   
                stock = await _finacialModelingPrepService.FindStockBySymbolAsync(symbol, cancellationToken);
                /* FMP API u searchCompanies u FE prikazuje sve Stocks i ETFs koji sadrze "tsla",ipak necu moci dodati bilo koji, jer neki od njih su ETF (a ne Stock) zato sto FMP API in FindStockBySymbolAsync pretrazuje samo Stocks ! */
                if (stock is null) 
                    return BadRequest("Ovo sto pokusavas dodati nije Stock, vec ETF, iako ga vidis u listu kad search uradis u FE. Jer API u BE pretrazuje samo Stocks (ne i ETFs). Dok SearchCompanies u FE pretrazuje FMP API koji prikazuje firme a one mogu biti Stock ili ETF.  ");
                    // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a "Nepostojeci stock symbol koji nema ni na netu" u Response Body.
                else 
                    await _stockRepository.CreateAsync(stock, cancellationToken); // stock je azuriran sa Id poljem, jer u CreateAsync EF tracking je to odradio. Sad stock je azuriran i ovde jer je reference type 
            }

            // Ako izabrani symbol nije Stock, vec ETF, iznad izlazimo iz ove metode zbog return. Ali ako je to Stock, onda proveravam da li on vec postoji u listi stocks of current appUser
            var userStocks = await _portfolioRepository.GetUserPortfoliosAsync(appUser, cancellationToken); // Lista svih Stocks koje appUser ima 
            if (userStocks.Any(e => e.Symbol.ToLower() == symbol.ToLower())) 
                return BadRequest("Cannot add same stock to portfolio as this user has already this stock in portfolio");
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a "Cannot add same stock to portfolio as this user has already this stock in portfolio" u Response Body.

            // Ako izabrani stock postoji, nebitno da l u bazi ili u FMP API, dodajemo ga u listu stockova za appUser
            var portfolio = new Portfolio // Jer Stock je jedan Portfolio
            {
                StockId = stock.Id,
                AppUserId = appUser.Id,
                Stock = stock,
                AppUser = appUser,
            };

            await _portfolioRepository.CreateAsync(portfolio, cancellationToken); // ne treba mi da dohvatim povratnu vrednost iz CreateAsync, jer portfolio je reference type pa je njegova promena u CreateAsync applied i ovde odma

            if (portfolio is null)
                return StatusCode(500, "Could not create Portfolio");
                // Frontendu ce biti poslato StatusCode=500 u Response Status Line, a "Could not create Portfolio" u Response Body.
            else
                return Created();
                // Frontendu ce biti poslato StatusCode=201 u Response Status Line, a Response Body prazan.
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeletePortfolio([FromQuery] string symbol, CancellationToken cancellationToken) // 1 Portfolio = 1 Stock, a glavna stvar Stock-a je Symbol polje
        {   // I da nisam stavio [Authorize], zbog User.GetUserName moral bi da se JWT prosledi sa Frontend prilikom gadjanja ovog Endpoint, ali treba staviti [Authorize] jer osigurava da userName!=null, jer forsira Frontend da salje JWT.
            // Da nema [FromQuery], obzirom da symbol je string, .NET bi prihvatamo i [FromRoute], [FromQuery] i [FromBody]. Zbog [FromQuery] u portfolioDeleteApi u FE moram poslati symbol nakon ? in URL
            
            var userName = User.GetUserName(); // Objasnjeno iznad
            var appUser = await _userManager.FindByNameAsync(userName); // Ne podrzava cancellationToken

            var userStocks = await _portfolioRepository.GetUserPortfoliosAsync(appUser, cancellationToken); 
            
            var stockInPortfolios = userStocks.Where(s => s.Symbol.ToLower() == symbol.ToLower()).ToList(); // Nema treba async, jer ne pretrazujem u bazu, vec in-memory userStocks varijablu

            // Ovaj if-else je dobra praksa za ne daj boze, ali sam vec u AddPortfolio obezbedio da moze samo 1 isti stock biti u listi
            if (stockInPortfolios.Count == 1) // is not null jer samo 1 Stock unique stock moze biti u portfolios list, jer AddPortfolio metoda iznad to ogranicila
                await _portfolioRepository.DeletePortfolio(appUser, symbol, cancellationToken);

            else
                return BadRequest("Stock is not in your portfolio list");
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a "Stock is not your portfolio list" u Response Body.

            return Ok();
            // Frontendu ce biti poslato StatusCode=200 u Response Status Line, a Response Body prazan.
        }
    }
}
