using Api.DTOs.CommentDTOs;
using Api.Extensions;
using Api.Helpers;
using Api.Interfaces;
using Api.Mapper;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    // Postman/Swagger gadja Endpoints ovde definisane

    [Route("api/comment")] // https://localhost:port/api/comment
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository; // U Program, definisano da ICommentRepository se odnosi na CommentRepository
        private readonly IStockRepository _stockRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IFinacialModelingPrepService _finacialModelingPrepService;

        public CommentController(ICommentRepository commentRepository, IStockRepository stockRepository, UserManager<AppUser> userManager, IFinacialModelingPrepService finacialModelingPrepService)
        {   // U Program.cs napisano da prepozna ICommentRepositor/IStockRepository kao CommentRepository/StockRepository
            _commentRepository = commentRepository;
            _stockRepository = stockRepository;
            _userManager = userManager;
            _finacialModelingPrepService = finacialModelingPrepService;
        }

        // Za Read from DB koristim CommentDTO klase kad gadjam Endpoint iz Frontend, nikad Comment jer ona je rezervisana za Repository tj za EF.
        // Za Write to DB koristim Create/UpdateCommentRequestDTO kad gadjam Endpoint iz Frontend, pa tek onda Comment koristim u Repository. 

        /* Svaki Endpoint bice tipa Task<IActionResult<T>> jer IActionResult<T> omoguci return of StatusCode + Data of type T, dok Task omogucava async. 
           
           Svaki Endpoint, salje to Frontend, Response koji ima polja Status Line, Headers i Body. 
        Status i Body najcesce definisem ja, a Header mogu ja u CreatedAtAction, ali najcesce to automatski .NET radi.
        Ako u objasnjenju return naredbe ne spomenem Header, to znaci da je on automatski popunjem podacima.

         Endpoint kad posalje Frontendu StatusCode!=2XX i mozda error data uz to, takav Response nece ostati u try block, vec ide u catch block i onda response=undefined u FE.
        */

        // Get All Comments Endpoint 
        [HttpGet]   // https://localhost:port/api/comment
        [Authorize] // Moram se login i uneti JWT u Authorize dugme u Swagger da bi mogo da pokrenem ovaj Endpoint, a u Frontend moram poslati JWT u request header kad gadjam ovaj Endpoint.
        public async Task<IActionResult> GetAll([FromQuery]CommentQueryObject commentQueryObject)
        {   /* Mora [FromQuery], a ne [FromBody], jer Axios GET Request u ReactTS ne moze imati Body, vec samo Header.
             U FE ovu metodu gadjam u commentGetAPI funkciji i, kroz Header, moram proslediti vrednosti za svako polje iz CommentQueryObject (iako su neka default value) redosledom i imenom iz CommentQueryObject

               U ReactTS Frontend, zbog ovog [Authorize], moram proslediti i JWT (kroz header), a kroz Request body redom i imenom polja iz CommentQueryObject moram navesti. 
            */
            var comments = await _commentRepository.GetAllAsync(commentQueryObject); 
            var commentDTOs = comments.Select(x => x.ToCommentDTO());

            return Ok(commentDTOs); // 200OK + list of CommentDTO
           // Frontendu ce biti poslato CommentDTO objekat u Response Body, a StatusCode=200 u Response Status Line.

        }

        // Get Comment By Id Endpoint
        [HttpGet("{id:int}")] // https://localhost:port/api/comment/{id}
        
        public async Task<IActionResult> GetById([FromRoute] int id)
        // Objasnjeno u StockController 
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment is null)
                return NotFound();
                // Frontendu ce biti poslato StatusCode=404 u Response Status Line, dok u Response Body nema nista.

            return Ok(comment.ToCommentDTO());
            // Frontendu ce biti poslato comment.ToCommentDTO() (tj CommentDTO objekat) u Response Body, a StatusCode=200 u Response Status Line.
        }

        [HttpPost("{symbol:alpha}")] // https://localhost:port/api/comment/{symbol} 
        // [HttpPost("{symbol}")] - moze i ovo
        // Ne sme [HttpPost("{symbol:string}")] jer gresku daje, obzirom da za string mora ili [HttpPost("{symbol:alpha}")] ili [HttpPost("{symbol}")] 
        //[Authorize] Necu staviti, jer User.GetUserName() svakako zahteva JWT, ali je dobra praksa imati [Authorize] u tom slucaju, da onaj ko cita kod, a ne zna dobro .NET, zna da mora JWT biti poslat.
        public async Task<IActionResult> Create([FromRoute] string symbol, CreateCommentRequestDTO createCommentRequestDTO)
        // Objasnjeno u StockController. Ako ne navedem [FromBody] to se podrazuvema da moram proslediti kroz Request Body taj parametar.
        {
            /* U ReactTS Fronted Request body moram da prosledim imena i redosled polja kao u CreateCommentRequestDTO kako bih uspesno pokrenuo ovaj endpoint, a
             dok symbol prosledim kroz URL zbgog [FromRoute].*/
            
            // Pokrece Data Validation in CreateCommentRequestDTO 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a ModelState objekat sa poljima EmailAddress, UserName i Password i svakom polju pisace koja je greska u njemu u Response Body.


            var stock = await _stockRepository.GetBySymbolAsync(symbol); // Await, jer obraca se bazi
            // Ako nije naso Stock u bazi, skida ga sa neta pomocu FinancialModelingPrepService FindStockBySymbolAsync, pa ga ubaca u bazu, pa onda uzima ga iz baze 
            if ( stock is null)
            {
                stock = await _finacialModelingPrepService.FindStockBySymbolAsync(symbol);
                if (stock is null) // AKo nije ga naso na netu
                    return BadRequest("Nepostojeci stock symbol koji nema ni na netu");
                    // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a "Nepostojeci stock symbol koji nema ni na netu" u Response Body.

                else // Ako ga naso na netu, ubaca ga u bazu
                    await _stockRepository.CreateAsync(stock); 
            }

            // I da ne stoji [Authorize] iznad, moram sa Frontend poslati JWT koji sam dobio zbog User.GetUserName(), ali je dobra praksa zbog ovoga imati [Authorize] jer obavezuje Frontend da posalje JWT da bi userName bio !=null
            var userName = User.GetUserName();  // User i GetUserName come from ControllerBase Claims  i odnose se na current logged user
            var appUser = await _userManager.FindByNameAsync(userName); 

            var comment = createCommentRequestDTO.ToCommentFromCreateCommentRequestDTO(stock.Id);
            comment.AppUserId = appUser.Id; 

            await _commentRepository.CreateAsync(comment);

            // Objasnjeno u StockController
            return CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment.ToCommentDTO());
            /* Prva 2 su route i route argument, jer GetById zahteva id argument.
               Frontendu ce biti poslato comment.ToCommentDTO() (tj CommentDTO objekat) u Response Body, StatusCode=201 u Response Status Line, a https://localhost:port/api/comment/{id} u Response Header.
            */
        }

        [HttpDelete("{id:int}")] // https://localhost:port/api/comment/{id}
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var comment = await _commentRepository.DeleteAsync(id);
            if (comment is null)
                return NotFound("Comment does not exist");
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a "Comment does not exists" u Response Body.

            return Ok(comment.ToCommentDTO()); 
            // Frontendu ce biti poslato comment.ToCommentDTO() (tj CommentDTO objekat) u Response body, a StatusCode=200 u Response Status Line.
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCommentRequestDTO updateCommentRequestDTO)
        {
            // Pokrece Data Validation in UpdateCommentRequestDTO 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comment = await _commentRepository.UpdateAsync(id, updateCommentRequestDTO.ToCommentFromUpdateCommentRequestDTO());
            if (comment is null)
                return NotFound("Comment not found");
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a "Comment not found" u Response Body.

            return Ok(comment.ToCommentDTO());
            // Frontendu ce biti poslato comment.ToCommentDTO() (tj CommentDTO objekat) u Response Body, a StatusCode=200 u Response Status Line.
        }
    }
}
