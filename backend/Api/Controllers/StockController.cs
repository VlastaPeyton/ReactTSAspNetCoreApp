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
    // Postman/Swagger gadja Endpoints ovde definisane

    [Route("api/stock")] // https://localhost:port/api/stock
    [ApiController]
    public class StockController : ControllerBase
    {   
        private readonly IStockRepository _stockRepository; // Interactions with DB are made inside Repository i tamo koristim ApplicationDbContext
        public StockController(IStockRepository repository)
        {   // U Program.cs napisano da prepozna IStockRepository kao StockRepository
            _stockRepository = repository;
        }

        // Za Read from DB koristim StockDTO klase, nikad Stock. 
        // Za Write to DB koristim prvo Create/UpdateStockReqeustDTO, pa onda Stock u Repository. 

        // Svaki Endpoint bice tipa IActionResult jer on omogucava return of HTTP Status + Data 

        // Get All Stocks Endpoint
        [HttpGet] // https://localhost:port/api/stock/
        [Authorize] // Mora u Swagger Authorize dugme da unesemo JWT token koji sam dobio prilikom login da bih mogo da pokrenem ovaj Endpoint
        public async Task<IActionResult> GetAll([FromQuery] QueryObject query) // Pogledaj QueryObject i bice jasno 
        {
            var stocks = await _stockRepository.GetAllAsync(query); 
            var stockDTOs = stocks.Select(s => s.ToStockDTO()).ToList(); // Nema async, jer stocks nije u bazi, vec in-memory jer smo ga vec dohvatili iz baze

            return Ok(stockDTOs); // 200OK + list of StockDTO elements
        }

        // Get Stock Endpoint
        [HttpGet("{id:int}")] // https://localhost:port/api/stock/{id}
        public async Task<IActionResult> GetById([FromRoute] int id) 
        // Mora bas "id" kao u liniji iznad i mora [FromRoute] jer id prosledjujem kroz URL, a ne kroz Postman body (JSON)
        {
            var stock = await _stockRepository.GetByIdAsync(id);
            if (stock == null)
                return NotFound(); 

            return Ok(stock.ToStockDTO()); // 200OK + StockDTO 
        }

        // Post Stock Endpoint
        [HttpPost] // https://localshost:port/api/stock
        public async Task<IActionResult> Create([FromBody] CreateStockRequestDTO createStockRequestDTO)
        // Mora [FromBody] jer ne prosledjujem argumente kroz URL (posto ih ima vise od 1 non primary tipa), vec kroz Postman body (JSON). Ovo je objekat i ne moze kroz URL nikad.
        // Mora CreateStockRequestDTO, jer Request object gadja Endpoint, stoga u Postman body kucam polja iz CreateStockRequestDTO.
        {
            // Pokrece Data Validation in CreateStockRequestDTO 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stock = createStockRequestDTO.ToStockFromCreateStockRequestDTO();
            await _stockRepository.CreateAsync(stock);
            
            // Vraca route/Endpoint (https://localhost:port/api/stock/{id}) sa "id" argumentom, jer baza automatski generisala Id za novi stock koji je upisan i novi stock u StockDTO obliku 
            return CreatedAtAction(nameof(GetById), new { id = stock.Id }, stock.ToStockDTO());
            // Iz nekog razloga id ne prati redosled, ali radi. 
        }
         
        // Update entire Stock Endpoint i zato PUT, a ne PATCH
        [HttpPut("{id:int}")] // https://localhost:port/api/stock/{id}
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStockRequestDTO updateStockRequestDTO)
        // Isto objasnjenje kao za Create.
        {
            // Pokrece Data Validation in UpdateStockRequestDTO 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stock = await _stockRepository.UpdateAsync(id, updateStockRequestDTO.ToStockFromUpdateStockRequestDTO());

            if (stock is null)
                return NotFound();

            return Ok(stock.ToStockDTO());
        }

        // Delete Stock Endpoint 
        [HttpDelete("{id:int}")] // https://localhost:port/api/stock/{id}
        public async Task<IActionResult> Delete([FromRoute] int id)
        // Na osnovu id brisem i onda [FromRoute] je dovoljno
        {
            var stock = await _stockRepository.DeleteAsync(id);

            if (stock is null)
                return NotFound();

            return NoContent();
        }
    }

}
