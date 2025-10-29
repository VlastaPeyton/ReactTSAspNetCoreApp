using Api.CQRS_and_Validation.Comment;
using Api.CQRS_and_Validation.Comment.Delete;
using Api.DTOs.CommentDTOs;
using Api.Extensions;
using Api.Helpers;
using Api.Interfaces;
using Api.Mapper;
using Api.Models;
using Mapster;
using MediatR;
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
    {
        private readonly ICommentService _commentService;
        public CommentController(ICommentService commentService) => _commentService = commentService;

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

         Koristim mapper extensions da napravim Comment Entity klasu from DTO kad pokrecem Repository metode ili napravim DTO from Comment Entity kad saljem data to FE.
         Controller radi mapiranje entity klasa u DTO osim ako koristim CQRS, jer nije dobro da repository vrati DTO obzriom da on radi sa domain i treba samo za entity klase da zna

         Za async endpoints nisam koristio cancellationToken = default, jer ako ReactTS pozove ovaj endpoint, i user navigates away or closes app, .NET ce automtaski da shvati da treba prekinuti izvrsenje i dodelice odgovarajucu vrednost tokenu. 
        Zbog nemanja "=default" u async endpoint, ne smem imati ni u await metodama koje se pozivaju u endpointu. 
        Da sam koristio "=default" ovde, .NET ne bi znao da automatski prekine izvrsenje endpointa, pa bih morao u FE axios metodi da prosledim i controller.signal...
        CancellationToken se stavlja za time-consuming await metode npr duga ocitavanja u bazi, ali ja cu staviti na sve, zlu ne trebalo.
         
        U 2 endpoint koristim CQRS, pa bih onda morao i svuda da ga koristim i da neam repository ovde, vec samo u Handler klasama, ali nema veze, CQRS mi i ne treba ovde, vec samo da pokazem
         Rate Limiter objasnjen u Program.cs
        
        Endpoint koji sadrzi "User.GetUserName" zahteva od FE da posalje JWT jer u JWT su upisane claims (user info) bez obzira da li ima [Authorize] ili nema.
        
        Sve vezano za GlobalExceptionHandler i Result pattern i Services je objasnjeno u AccountConttroller.
        */

        // Get All Comments for desired Stock Endpoint 
        [HttpGet]   // https://localhost:port/api/comment
        [Authorize] // Moram se login i uneti JWT u Authorize dugme u Swagger da bi mogo da pokrenem ovaj Endpoint
        public async Task<IActionResult> GetAll([FromQuery] CommentQueryObject commentQueryObject, CancellationToken cancellationToken)
        {   /* Mora [FromQuery], jer GET Axios Request u ReactTS ne moze imati Body, vec samo Header, pa ne moze [FromBody]. 
               Kroz Query Parameters u FE (posle ? in URL), moram proslediti vrednosti za svako polje iz CommentQueryObject (iako neka imaju default value) redosledom i imenom iz CommentQueryObject
               U ReactTS Frontend, zbog [Authorize], moram proslediti i JWT kroz Request Header u commentsGetAPI funkciji.
            */
            var commentDTOs = await _commentService.GetAllAsync(commentQueryObject, cancellationToken);

            return Ok(commentDTOs); 
           // Frontendu ce biti poslato commentDTOs lista u Response Body, a StatusCode=200 u Response Status Line.
        }

        // Get Comment By Id Endpoint
        [HttpGet("{id:int}")] // https://localhost:port/api/comment/{id}
        [Authorize]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken, [FromServices] ISender _sender)
        // Mora bas "id" ime kao u liniji iznad i moze [FromRoute] jer id obicno prosledim kroz URL, a ne kroz Request body (JSON) ili Query
        {
            // Koristim CQRS, a ne Service cisto da pokazem znanje.

            // Ne mapiram Request to Query, jer Request nema potrebe zbog jednog argumenta primitivnog tipa da postoji, vec odma Query objekat pravim i saljem u MediatR pipeline
            var result = await _sender.Send(new CommentGetByIdQuery(id)); // result = CommentGetByIdResult
            if (result.IsFailure)
                return NotFound(new {message = result.Error});

            // MediatR ne poziva ValidationBehavior jer Validation samo za ICommand napravljeno, pa onda odma zove CommentGetByIdQueryHandler Handle metodu
            var response = result.Adapt<CommentGetByIdResponse>(); // Mapster auto mapira jer su polja istog imena i tipa u obe klase. Mogo sam i bez Response, ali cisto da vidite kako izgleda.
            return Ok(response.commentDTOResponse);
        }

        //[EnableRateLimiting("slow")]
        [HttpPost("{symbol:alpha}")] // https://localhost:port/api/comment/{symbol} 
        // Ne sme [HttpPost("{symbol:string}")] jer gresku daje, obzirom da za string mora ili [HttpPost("{symbol:alpha}")] ili [HttpPost("{symbol}")] 
        [Authorize] // I da nisam stavio [Authorize], zbog User.GetUserName() moralo bi da se JWT prosledi sa Frontend prilikom gadjanja ovog Endpoint, ali treba staviti [Authorize] jer osigurava da userName!=null, jer forsira Frontend da salje JWT.

        public async Task<IActionResult> Create([FromRoute] string symbol, [FromBody] CreateCommentRequestDTO createCommentRequestDTO, CancellationToken cancellationToken)
        // U FE commentPostAPI funkciji, symbol kroz URL prosledim, a kroz Body saljem polja imenom i redosledom kao u CreateCommentRequestDTO (nisam stavio [FromBody] jer se to podrazumeva za complex type in POST request)
        {
            // ModelState pokrene validation za CreateCommentRequestDTO tj za zeljena CreateCommentRequestDTO polja proverava na osnovu onih annotation iznad polja koje stoje. ModelState se koristi za Writing to DB.
            if (!ModelState.IsValid)             
                return BadRequest(ModelState);
                // Frontendu ce biti poslato StatusCode=400 u Response Status, a ModelState ce biti poslat u Response Body sa CreateCommentRequestDTO poljima u "errors" delu of Response

            /* User i GetUserName come from ControllerBase, jer User je ClaimsPrincipal i odnosi se na current logged user (HttpContext.User) jer mnogo je lakse uzeti UserName/Email iz Claims (in-memory) nego iz baze i zahteva od FE da posalje JWT jer su u njemu claims
               User i GetUserName su http sloj, i ne mogu se proslediti u CreateAsync metodu servisa, vec userName prosledim. */
            var userName = User.GetUserName();

            var result = await _commentService.CreateAsync(userName, symbol, createCommentRequestDTO, cancellationToken);
            if (result.IsFailure)
                return BadRequest("Nepostojeci stock symbol koji nema ni na netu ili FMP API ne radi mozda");

            var commentDTOResponse = result.Value; 

            return CreatedAtAction(nameof(GetById), new { id = commentDTOResponse.Id }, commentDTOResponse); // Id property of Comment ima Value polje jer strongly-id type
            /* Prva 2 su route of GetById endpoint i endpoint's argument, jer GetById endpoint zahteva id argument.
               Frontendu ce biti poslato comment.ToCommentDTO() (tj CommentDTO objekat) u Response Body, StatusCode=201 u Response Status Line, a https://localhost:port/api/comment/{id} u Response Header.
            */
        }

        [HttpDelete("{id:int}")] // https://localhost:port/api/comment/{id}
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken, [FromServices] ISender _sender)
        {
            // Koristim CQRS, a ne Service cisto da pokazem znanje.

            // Zbog authorization u CQRS kako user moze samo svoj komentar brisati 
            var userName = User.GetUserName(); 

            // Ne mapiram Request to Query, jer Request nema potrebe zbog jednog argumenta primitivnog tipa da postoji, vec odma Query objekat pravim i saljem u MediatR pipeline
            var result = await _sender.Send(new CommentDeleteCommand(id, userName));
            // MediatR poziva ValidationBehaviour (ubacen u pipeline kroz u Program.cs) jer je ovo Command i jos neke pipeline behaviours ako ih ima (a nema), pa tek na kraju CommentDeleteCommandHandler's Handle metodu
            // Ne treba mi CommentDeleteResult to CommentDeleteResponse mapping 
            return Ok(result.commentDTOResponse);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCommentRequestDTO updateCommentRequestDTO, CancellationToken cancellationToken)
        // U FE, id uvek saljem in URL, dok complex type kroz Body kao sto znam
        {
            // ModelState pokrene validation za UpdateCommentRequestDTO tj za zeljena CreateCommentRequestDTO polja proverava na osnovu njihovih annotations. ModelState se koristi za Writing to DB.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a ModelState objekat  biti poslat u Response Body sa UpdateCommentRequestDTO poljima u "errors" delu of Response
            
            var result = await _commentService.UpdateAsync(id, updateCommentRequestDTO, cancellationToken);
            if (result.IsFailure)
                return NotFound("Comment not found");
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a "Comment not found" u Response Body.

            var commentDTOResponse = result.Value;

            return Ok(commentDTOResponse);
            // Frontendu ce biti poslato comment.ToCommentDTO() (tj CommentDTO objekat) u Response Body, a StatusCode=200 u Response Status Line.
        }
    }
}
