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

        // Za Read from DB koristim CommentDTO klase, nikad Comment. 
        // Za Write to DB koristim Create/UpdateCommentRequestDTO, pa tek onda Comment u Repository. 
        // Svaki Endpoint bice tipa IActionResult jer on omogucava return of HTTP Status + Data 
        
        // Get All Comments Endpoint 
        [HttpGet]  // https://localhost:port/api/comment
        [Authorize] // Moram se login i uneti JWT u AUthorize dugme u Swagger da bi mogo da pokrenem ovaj Endpoint
        public async Task<IActionResult> GetAll([FromQuery]CommentQueryObject commentQueryObject)
        {   /* Mora [FromQuery] a ne [FromBody] jer HttpGet u React ne moze imati body. 
             
               U ReactTS Frontend moram proslediti i JWT zbog [Authorize] i kroz Request body sve redom i imenom polja iz 
             CommentQueryObject. */
            var comments = await _commentRepository.GetAllAsync(commentQueryObject); 
            var commentDTOs = comments.Select(x => x.ToCommentDTO());

            return Ok(commentDTOs); // 200OK + list of CommentDTO
        }

        // Get Comment By Id Endpoint
        [HttpGet("{id:int}")] // https://localhost:port/api/comment/{id}
        public async Task<IActionResult> GetById([FromRoute] int id)
        // Objasnjeno u StockController 
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment is null)
                return NotFound();

            return Ok(comment.ToCommentDTO());
        }

        [HttpPost("{symbol}")] // https://localhost:port/api/comment/{symbol} 
        // Ne sme [HttpPost("{symbol:string}")] jer gresku daje 
        [Authorize] 
        public async Task<IActionResult> Create([FromRoute] string symbol, CreateCommentRequestDTO createCommentRequestDTO)
        // Objasnjeno u StockController 
        {
            /* U ReactTS Fronted, moram da prosledim imena i redosled polja kao u CreateCommentRequestDTO u Request body kako bi uspesno pokrenuo ovaj endpoint. N
             Dok symbol prosledim kroz URL.*/
            
            // Pokrece Data Validation in CreateCommentRequestDTO 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stock = await _stockRepository.GetBySymbolAsync(symbol); // Await, jer obraca se bazi
            // Ako nije naso Stock u bazi, skida ga sa neta pomocu FinancialModelingPrepService FindStockBySymbolAsync, pa ga ubaca u bazu, pa onda uzima ga iz baze 
            if ( stock is null)
            {
                stock = await _finacialModelingPrepService.FindStockBySymbolAsync(symbol);
                if (stock is null) // AKo nije ga naso na netu
                    return BadRequest("Nepostojeci stock symbol koji nema ni na netu");
                else // Ako ga naso na netu, ubaca ga u bazu
                    await _stockRepository.CreateAsync(stock); 
            }

            // I da ne stoji [Authorize] iznad, moram sa Frontend poslati JWT koji sam dobio zbog ovoga
            var userName = User.GetUserName();  // User i GetUserName come from ControllerBase Claims  i odnose se na current logged user
            var appUser = await _userManager.FindByNameAsync(userName); 

            var comment = createCommentRequestDTO.ToCommentFromCreateCommentRequestDTO(stock.Id);
            comment.AppUserId = appUser.Id; 

            await _commentRepository.CreateAsync(comment);

            // Objasnjeno u StockController
            return CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment.ToCommentDTO());
        }

        [HttpDelete("{id:int}")] // https://localhost:port/api/comment/{id}
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var comment = await _commentRepository.DeleteAsync(id);
            if (comment is null)
                return NotFound("Comment does not exist");

            return Ok(comment.ToCommentDTO()); 
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

            return Ok(comment.ToCommentDTO());
        }
    }
}
