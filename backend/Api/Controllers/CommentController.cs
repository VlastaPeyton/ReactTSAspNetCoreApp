using Api.DTOs.CommentDTOs;
using Api.Extensions;
using Api.Helpers;
using Api.Interfaces;
using Api.Mapper;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers
{
    // Postman/Swagger/FE gadja Endpoints ovde definisane

    [Route("api/comment")] // https://localhost:port/api/comment
    [ApiController]
    public class CommentController : ControllerBase
    {    // Interface za sve klase zbog DI, dok u Program.cs napisem da prepozna interface kao tu klasu
        private readonly ICommentRepository _commentRepository; 
        private readonly IStockRepository _stockRepository; // Interaction with DB is made inside Repository
        private readonly UserManager<AppUser> _userManager; // Ovo moze jer AppUser:IdentityUser 
        private readonly IFinacialModelingPrepService _finacialModelingPrepService;

        public CommentController(ICommentRepository commentRepository, IStockRepository stockRepository, UserManager<AppUser> userManager, IFinacialModelingPrepService finacialModelingPrepService)
        {   // U Program.cs registrovan da prepozna ICommentRepositor/IStockRepository/IFinacialModelingPrepService kao CommentRepository/StockRepository/FinacialModelingPrepService
            _commentRepository = commentRepository;
            _stockRepository = stockRepository;
            _userManager = userManager;
            _finacialModelingPrepService = finacialModelingPrepService;
        }

        // Za Endpoint argument koristim DTO klase i za return object to FE, nikad Comment tj Models klase, jer Models klase namenjene za Repository tj za EF.

        /* Svaki Endpoint bice tipa Task<IActionResult<T>> jer IActionResult<T> omoguci return of StatusCode + Data of type T, dok Task omogucava async. 
           
           Svaki Endpoint, salje to Frontend, Response koji ima polja Status Line, Headers i Body. 
        Status i Body najcesce definisem ja, a Header mogu ja u CreatedAtAction, ali najcesce to automatski .NET radi.
        Ako u objasnjenju return naredbe ne spomenem Header, to znaci da je on automatski popunjem podacima.

         Endpoint kad posalje Frontendu StatusCode!=2XX i mozda error data uz to, takav Response nece ostati u try block, vec ide u catch block i onda response=undefined u FE.
         
         Data validation when writing to DB: 
            Request object je Endpoint argument koji stize from FE in order to write/read DB. Request object is never Entity class, but DTO class as i want to split api from domain/infrastructure layer. 
            If Endpoint exhibits write to DB, i have to validate Request DTO object fields before it is mapped to Entity class and written to DB. 
            Validation can be done using ModelState - u Write to DB Endpoint stavim ModelState koji zna da treba da validira annotated polja iz Request DTO object iz tog Endpoint. ModelState ako ne zelim custom validation logic.
            Validation can be done using FluentValidation - ako zelim custom validaiton logic.          
         
         Ne koristim FluentValidation jer za sad nema potrebe, a samo ce da mi napravi kod more complex. Koristim ModelState.

         Ako Endpoint nema [Authorize] ili User.GetUserName(), u FE ne treba slati JWT in Request Header, ali ako ima bar 1, onda treba.

         Koristim mapper extensions da napravim Comment Entity klasu from DTO kad pokrecem Repository metode ili napravim DTO from Comment Entity kad saljem data to FE.
         
         Za async Endpoints, nisam koristio cancellationToken = default, jer ako ReactTS pozove ovaj Endpoint, i user navigates away or closes app, .NET ce automtaski da shvati da treba prekinuti izvrsenje i dodelice odgovarajucu vrednost tokenu. 
        Zbog nemanja =default ovde, ne smem imati ni u await metodama koje se pozivaju u Endpointu. 
        Da sam koristio =default ovde, .NET ne bi znao da automatski prekine izvrsenje Endpointa, pa bih morao u FE axios metodi da prosledim i controller.signal...
        CT se stavlja za time-consuming await metode npr duga ocitavanja u bazi, ali ja cu staviti na sve, zlu ne trebalo.
         
         Rate Limiter objasnjen u Program.cs
        */

        // Get All Comments for desired Stock Endpoint 
        [HttpGet]   // https://localhost:port/api/comment
        [Authorize] // Moram se login i uneti JWT u Authorize dugme u Swagger da bi mogo da pokrenem ovaj Endpoint
        public async Task<IActionResult> GetAll([FromQuery]CommentQueryObject commentQueryObject, CancellationToken cancellationToken)
        {   /* Mora [FromQuery], jer GET Axios Request u ReactTS ne moze imati Body, vec samo Header, pa ne moze [FromBody]. Kroz Query Parameters u FE (posle ? in URL), moram proslediti vrednosti za svako polje iz CommentQueryObject (iako neka imaju default value) redosledom i imenom iz CommentQueryObject
               U ReactTS Frontend, zbog [Authorize], moram proslediti i JWT kroz Header u commentsGetAPI funkciji.
            */
            var comments = await _commentRepository.GetAllAsync(commentQueryObject, cancellationToken); 
            var commentDTOs = comments.Select(x => x.ToCommentDTO());

            return Ok(commentDTOs); 
           // Frontendu ce biti poslato commentDTOs lista u Response Body, a StatusCode=200 u Response Status Line.

        }

        // Get Comment By Id Endpoint
        [HttpGet("{id:int}")] // https://localhost:port/api/comment/{id}
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
        // Mora bas "id" kao u liniji iznad i moze [FromRoute] jer id obicno prosledim kroz URL, a ne kroz Request body (JSON)
        {
            var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
            if (comment is null)
                return NotFound();
                // Frontendu ce biti poslato StatusCode=404 u Response Status Line, dok u Response Body nema nista.

            return Ok(comment.ToCommentDTO());
            // Frontendu ce biti poslato comment.ToCommentDTO() (tj CommentDTO objekat) u Response Body, a StatusCode=200 u Response Status Line.
        }

        //[EnableRateLimiting("slow")]
        [HttpPost("{symbol:alpha}")] // https://localhost:port/api/comment/{symbol} 
        // Ne sme [HttpPost("{symbol:string}")] jer gresku daje, obzirom da za string mora ili [HttpPost("{symbol:alpha}")] ili [HttpPost("{symbol}")] 
        // [Authorize] Necu staviti, jer User.GetUserName() svakako zahteva JWT, ali je dobra praksa imati [Authorize] u tom slucaju, da onaj ko cita kod, a ne zna dobro .NET, zna da mora JWT biti poslat from FE.
        public async Task<IActionResult> Create([FromRoute] string symbol, CreateCommentRequestDTO createCommentRequestDTO, CancellationToken cancellationToken)
        // U FE commentPostAPI funkciji, symbol kroz URL prosledim, a kroz Body saljem polja imenom i redosledom kao u CreateCommentRequestDTO (nisam stavio [FromBody] jer se to podrazumeva za complex type in POST request)
        {
            // ModelState pokrene validation za CreateCommentRequestDTO tj za zeljena CreateCommentRequestDTO polja proverava na osnovu onih annotation iznad polja koje stoje. ModelState se koristi za Writing to DB.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a ModelState ce biti poslat u Response Body sa CreateCommentRequestDTO poljima u "errors" delu of Response

            // U FE, zelim da ostavim komentar za neki stock, pa u search kucam npr "tsla" i onda on trazi "tsla" sve stocks koji pocinju na "tsla" u bazi pomocu GetBySymbolAsync
            var stock = await _stockRepository.GetBySymbolAsync(symbol, cancellationToken); // Nadje u bazy stock za koji ocu da napisem komentar 
            // ako nije naso "tsla" stock u bazi, nadje ga na netu pomocu FinancialModelingPrepService, pa ga ubaca u bazu, pa onda uzima ga iz baze da bih mi se pojavio na ekranu i da mogu da udjem u njega da comment ostavim
            if (stock is null)
            {
                stock = await _finacialModelingPrepService.FindStockBySymbolAsync(symbol, cancellationToken);
                if (stock is null) // Ako nije ga naso na netu, onda smo lose ukucali u search
                    return BadRequest("Nepostojeci stock symbol koji nema ni na netu");
                    // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a "Nepostojeci stock symbol koji nema ni na netu" u Response Body.

                else // Ako ga naso na netu, ubaca ga u bazu
                    await _stockRepository.CreateAsync(stock, cancellationToken); 
            }

            // I da ne stoji [Authorize] iznad, moram sa FE slati JWT, zbog User.GetUserName(), ali je dobra praksa zbog ovoga imati [Authorize] jer obavezuje Frontend da posalje JWT da bi userName bio !=null
            var userName = User.GetUserName(); // User i GetUserName come from ControllerBase Claims i odnose se na current logged user jer mnogo je lakse uzeti UserName/Email iz Claims (in-memory) nego iz baze
            var appUser = await _userManager.FindByNameAsync(userName); // Pretrazi AspNetUser tabelu da nadje usera na osnovu userName
            // _userManager methods does not use cancellationToken

            var comment = createCommentRequestDTO.ToCommentFromCreateCommentRequestDTO(stock.Id);
            comment.AppUserId = appUser.Id; 

            await _commentRepository.CreateAsync(comment, cancellationToken); // Iako CreateAsync ima return, ne treba "var result = _commentRepository.CreateAsync(comment), jer comment je Reference type, stoga promena comment u CreateAsync uticace i ovde

            return CreatedAtAction(nameof(GetById), new { id = comment.Id.Value }, comment.ToCommentDTO()); // Id property of Comment ima Value polje jer strongly-id type
            /* Prva 2 su route i route argument, jer GetById zahteva id argument.
               Frontendu ce biti poslato comment.ToCommentDTO() (tj CommentDTO objekat) u Response Body, StatusCode=201 u Response Status Line, a https://localhost:port/api/comment/{id} u Response Header.
            */
        }

        [HttpDelete("{id:int}")] // https://localhost:port/api/comment/{id}
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.DeleteAsync(id, cancellationToken);
            if (comment is null)
                return NotFound("Comment does not exist");
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a "Comment does not exists" u Response Body.

            return Ok(comment.ToCommentDTO()); 
            // Frontendu ce biti poslato comment.ToCommentDTO() (tj CommentDTO objekat) u Response body, a StatusCode=200 u Response Status Line.
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCommentRequestDTO updateCommentRequestDTO, CancellationToken cancellationToken)
        // U FE, id obicno saljem in URL, dok complex type kroz Body 
        {
            // ModelState pokrene validation za UpdateCommentRequestDTO tj za zeljena CreateCommentRequestDTO polja proverava na osnovu onih annotation iznad polja koje stoje. ModelState se koristi za Writing to DB.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a ModelState objekat  biti poslat u Response Body sa UpdateCommentRequestDTO poljima u "errors" delu of Response
            var comment = await _commentRepository.UpdateAsync(id, updateCommentRequestDTO.ToCommentFromUpdateCommentRequestDTO(), cancellationToken);
            if (comment is null)
                return NotFound("Comment not found");
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a "Comment not found" u Response Body.

            return Ok(comment.ToCommentDTO());
            // Frontendu ce biti poslato comment.ToCommentDTO() (tj CommentDTO objekat) u Response Body, a StatusCode=200 u Response Status Line.
        }
    }
}
