using Api.Data;
using Api.DTOs.Stock;
using Api.Helpers;
using Api.Interfaces;
using Api.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{   
    // Postman/Swagger/FE gadja Endpoints ovde definisane

    [Route("api/stock")] // https://localhost:port/api/stock
    [ApiController]
    public class StockController : ControllerBase
    {    // Interface za sve klase zbog DI, dok u Program.cs napisem da prepozna interface kao tu klasu
        private readonly IStockRepository _stockRepository; // Interactions with DB are made inside Repository
        public StockController(IStockRepository repository)
        {   // U Program.cs registrovan IStockRepository kao StockRepository
            _stockRepository = repository;
        }

        /* 
           Za svaki Request pravi se automatski nova instanca kontrolera (AddTransient fakticki), onda DI automatski, na osnovu AddTransient/Scoped/Singleton<IService,Service>() iz Program.cs, u ctor kontrolera doda zeljeni servis i kad se 
          response posalje u FE, GC unistava kontroler, ali zivot servisa zavisi da li je Singleton, Transient ili Scoped.

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
         
         Koristim mapper extensions da napravim Stock Entity klasu from DTO kad pokrecem Repository metode ili napravim DTO from Stock Entity kad saljem data to FE.
         
         Za async Endpoints, nisam koristio cancellationToken = default, jer ako ReactTS pozove ovaj Endpoint, i user navigates away or closes app, .NET ce automtaski da shvati da treba prekinuti izvrsenje i dodelice odgovarajucu vrednost tokenu. 
        Zbog nemanja "=default" ovde, ne smem imati ni u await metodama koje se pozivaju u Endpointu. 
        Da sam koristio =default ovde, .NET ne bi znao da automatski prekine izvrsenje Endpointa, pa bih morao u FE axios metodi da prosledim i controller.signal...
        CancellationToken se stavlja za time-consuming await metode npr duga ocitavanja u bazi, ali ja cu staviti na sve, zlu ne trebalo.
        
         Rate Limiter objasnjen u Program.cs
        */

        // Get All Stocks Endpoint
        [HttpGet]   // https://localhost:port/api/stock/
        [Authorize] // Mora u Swagger Authorize dugme da unesemo JWT token koji sam dobio prilikom login/register da bih mogo da pokrenem ovaj Endpoint + FE mora poslati JWT u Authorization header of request
        public async Task<IActionResult> GetAll([FromQuery] QueryObject query, CancellationToken cancellationToken)
        {   // Mora [FromQuery], jer GET Axios Request u ReactTS ne moze da ima body, vec samo Header, pa ne moze [FromBody]. Kroz Query Parameters u FE (posle ? in URL), moram proslediti vrednosti za svako polje iz QueryObject (iako neka imaju default value) redosledom i imenom iz QueryObject
            // U ReactTS zbog [Authorize] moram proslediti JWT u Request Header. 
            var stocks = await _stockRepository.GetAllAsync(query, cancellationToken); 
            var stockDTOs = stocks.Select(s => s.ToStockDTO()).ToList(); // Nema async, jer stocks nije u bazi, vec in-memory jer smo ga vec dohvatili iz baze

            return Ok(stockDTOs); 
            // Frontendu ce biti poslato StatusCode=200 u Response Status Line, a stockDTOs u Response Body.
        }

        // Get Stock Endpoint
        [HttpGet("{id:int}")] // https://localhost:port/api/stock/{id}
        [Authorize]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken) 
        // Mora bas "id" kao u liniji iznad i moze [FromRoute] jer id obicno prosledim kroz URL, a ne kroz Request body (JSON)
        {
            var stock = await _stockRepository.GetByIdAsync(id, cancellationToken);
            if (stock == null)
                return NotFound(); 
                // Frontendu ce biti poslato StatusCode=404 u Response Status Line, a Response Body je prazan.

            return Ok(stock.ToStockDTO()); 
            // Frontendu ce biti poslato StatusCode=200 u Response Status Line, a stock.ToStockDTO (tj StockDTO objekat) u Response Body.
        }

        // Post Stock Endpoint
        [HttpPost] // https://localshost:port/api/stock
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateStockRequestDTO createStockRequestDTO, CancellationToken cancellationToken)
        // Mora [FromBody] jer ne prosledjujem argumente kroz URL (posto ih ima vise od 1 non primary tipa), vec kroz Postman body (JSON).
        // Mora CreateStockRequestDTO, jer Request gadja Endpoint, stoga u Request body kucam polja iz CreateStockRequestDTO imenom i redosledom kao u CreateStockRequestDTO
        {
            // ModelState pokrene validation za CreateStockRequestDTO tj za zeljena CreateStockRequestDTO polja proverava na osnovu onih annotation iznad polja koje stoje. ModelState se koristi za Writing to DB.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a ModelState objekat bice poslat u Response Body sa CreateStockRequestDTO poljima u "errors" delu of Response

            var stock = createStockRequestDTO.ToStockFromCreateStockRequestDTO(); // Bez vrednosti u Id polju za sada 
            await _stockRepository.CreateAsync(stock, cancellationToken); // Iako CreateAsync ima return, ne treba "var result = _stockRepository.CreateAsync(stock), jer stock je Reference type, stoga promena stock u CreateAsync uticace i ovde
            
            return CreatedAtAction(nameof(GetById), new { id = stock.Id }, stock.ToStockDTO());
            // Iz nekog razloga id ne prati redosled, ali radi kako treba. 
            /* Prva 2 su route i route argument, jer GetById zahteva id argument.
               Frontendu ce biti poslato stock.ToStockDTO() (tj StockDTO objekat) u Response Body, StatusCode=201 u Response Status Line, a https://localhost:port/api/stock/{id} u Response Header.
            */
        }

        // Update entire Stock Endpoint i zato PUT, a ne PATCH (jer PATCH je update samo za zeljena polja)
        [HttpPut("{id:int}")] // https://localhost:port/api/stock/{id}
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStockRequestDTO updateStockRequestDTO, CancellationToken cancellationToken)
        // Id kroz URL obicno saljem iz FE i zato [FromRoute], dok objekte (JSON) moram kroz Body (ili kroz Query Parameters ako je Axios.GET u pitanju jer one podrzava Body).
        {
            // ModelState pokrene validation za UpdateStockRequestDTO tj za zeljena UpdateStockRequestDTO polja proverava na osnovu onih annotation iznad polja koje stoje.ModelState se koristi za writing to DB.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a ModelState objekat sa poljima EmailAddress, UserName i Password i svakom polju pisace koja je greska u njemu u Response Body u "errors" delu.

            var stock = await _stockRepository.UpdateAsync(id, updateStockRequestDTO.ToStockFromUpdateStockRequestDTO(), cancellationToken);

            if (stock is null)
                return NotFound();
                // Frontendu ce biti poslato StatusCode=404 u Response Sttus Line, a Response Body prazan.

            return Ok(stock.ToStockDTO());
            // Frontendu ce biti poslato StatusCode=200 u Response Status Line, a stock.ToStockDTO (tj StockDTO object) u Response Body.
        }

        // Delete Stock Endpoint 
        [HttpDelete("{id:int}")] // https://localhost:port/api/stock/{id}
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
        // Id kroz URL se obicno salje iz FE i zato [FromRoute]
        {
            var stock = await _stockRepository.DeleteAsync(id, cancellationToken);

            if (stock is null)
                return NotFound();
                // Frontendu ce biti poslato StatusCode=404 u Response Status Line, a Response Body prazan.

            return NoContent();
            // Frontendu ce biti poslato StatusCode=204 u Response Status Line, a Response Body prazan.
        }
    }

}
