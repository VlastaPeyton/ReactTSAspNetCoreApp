using Api.DTOs.Account;
using Api.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    // Postman/Swagger gadja Endpoints ovde definisane

    [Route("api/account")] // https://localhost:port/api/account
    [ApiController]
    public class AccountController : ControllerBase
    {   
        private readonly UserManager<AppUser> _userManager; // AppUser je nasledio IdentityUser
        private readonly SignInManager<AppUser> _signInManager; 
        private readonly ITokenService _tokenService; // U Program definisali da prepozna ITokenService kao TokenService
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;

        }

        // Svaki Endpoint prima DTO klase kao argumente, jer to je dobra praksa da ne diram Models klase koje su za Repository namenjene.

        /* Svaki Endpoint bice tipa Task<IActionResult<T>> jer IActionResult<T> omoguci return of StatusCode + Data of type T, dok Task omogucava async. 
           
           Svaki Endpoint, salje to Frontend, Response koji ima polja Status Line, Headers i Body. 
        Status i Body najcesce definisem ja, a Header mogu ja u CreatedAtAction, ali najcesce to automatski .NET radi.
        Ako u objasnjenju return naredbe ne spomenem Header, to znaci da je on automatski popunjem podacima.
            
         Endpoint kad posalje Frontendu StatusCode!=2XX i mozda error data uz to, takav Response nece ostati u try block, vec ide u catch block i onda response=undefined u ReactTS.
        */

        [HttpPost("register")] // // https://localhost:port/api/account/register
        // Ne ide [Authenticate] jer ovo je Register 
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {   /* Pre ove metode , pokrenuto je OnModelCreating iz ApplicationDBContext i napunjena je AspNetRoles tabela prilikom Migracije (ako sam migrirao).
               
               Kad novi User ukuca Email i Password, poziva se AspNetCoreIdentity da ga nadje u bazi kroz ApplicationDbContext : IdentityDbContext<AppUser> tj
            pretrazuje AspNetUsers tabelu i poredi uneti password sa stvarnim u toj tabeli. Moze da returnuje i podatke iz AspNetUserRoles i AspNetRoles i AspNetClaims tabela ako treba. 
            Tek nakon ovoga, poziva se CreateToken metoda iz TokenService da se JWT generise, jer JWT treba kako ne bi, za svaki API request, app gledao u DB kao prilikom user Registering sto uradi.  
            
               Kad gadjam iz ReactTS Frontenda ovaj Endpoint, moram polja da nazovem i prosledim redosledom kao u RegisterDTO jer RegisterDTO je tip input argumenta. Zbog [FromBody] moram u body of Request ih staviti.
             */


            // Try-Catch, jer cesto se desava server error when using UserManager
            try
            {   
                // Pokrene Data Validaiton iz RegisterDTO
                if(!ModelState.IsValid)
                    return BadRequest(ModelState); 
                    // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a ModelState objekat sa poljima EmailAddress, UserName i Password i svakom polju pisace koja je greska u njemu u Response Body.

                var appUser = new AppUser
                {
                    UserName = registerDTO.UserName,
                    Email = registerDTO.EmailAddress
                };

                var createdUser = await _userManager.CreateAsync(appUser, registerDTO.Password);
                // Dodaje AppUser polja (UserName i Email) i register.Password (koga automatski hash-uje) u AspNetUsers tabelu tj kreira novi User u toj tabeli
                // CreateAsync sprecava 2 usera sa istim UserName ili Email(Za email sam morao u Program da definisem rucno)

                if (createdUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(appUser, "User"); // Mogo sam samo User ili Admin upisati, jer samo te vrednosti su Seedovane Migracijom kroz OnModelCreating u AspNetRoles tabelu
                    // Dodaje u AspNetUserRoles tabelu koja automatski ima RoleId FK koji gadja Id u AspNetRoles tabeli koja je popunjena prilikom Migracije

                    if (roleResult.Succeeded)
                        return Ok(new NewUserDTO { UserName = appUser.UserName, EmailAddress = appUser.Email, Token = _tokenService.CreateToken(appUser)});
                        // Zbog OK, Frontendu ce biti poslato NewUserDTO u Response Body, a StatusCode=200 u Response Status Line.

                    else
                        return StatusCode(500, roleResult.Errors);
                        // Frontendu ce biti poslato StatusCode=500 u Response Status Line, a roleResult.Errors u Response Body.
                }
                else // Ako vec postoji user sa istim EmailAddres ili UserName
                {
                    return StatusCode(500, createdUser.Errors);
                    // Frontendu ce biti poslato StatusCode=500 u Response Status Line, a roleResult.Errors u Response Body.
                }

            } catch (Exception ex)
            {
                return StatusCode(500, ex);
                // Frontendu ce biti poslato StatusCode=500 u Response Status Line, a roleResult.Errors u Response Body.
            }
        }

        [HttpPost("login")] // https://localhost:port/api/account/login
        // Ne ide [Authorize] jer je ovo Login
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {   /* Kad existing User ukuca Email i Password, poziva se AspNetCoreIdentity da ga nadje u bazi kroz ApplicationDbContext : IdentityDbContext<AppUser> tj
            pretrazuje AspNetUsers tabelu i poredi uneti password sa stvarnim u toj tabeli. Moze da returnuje i podatke iz AspNetUserRoles i AspNetRoles i AspNetClaims tabela ako treba. 
            Tek nakon ovoga, poziva se CreateToken metoda iz TokenService da se JWT generise, jer JWT treba kako ne bi, za svaki API request, app gledao u DB kao prilikom user login sto uradi.
            
            Kad gadjam iz ReactTS Frontenda ovaj Endpoint, moram polja da nazovem i prosledim redosledom kao u LoginDTO jer LoginDTO je tip input argumenta. Zbog [FromBody] moram u body of Request ih staviti.
            */

            // Pokrene Data Validaiton iz LoginDTO
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a ModelState objekat sa poljima EmailAddress, UserName i Password i svakom polju pisace koja je greska u njemu u Response Body.

            //var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginDTO.UserName.ToLower()); // Moze i FindAsync jer je brze 
            // _userManager.Users odnsosi se na AspNetUsers tabelu
            var appUser = await _userManager.FindByNameAsync(loginDTO.UserName); // Bolji i sigurniji nacin nego linija iznad i takodje pretrazuje AspNetUsers tabelu

            if (appUser is null)
                return Unauthorized("Invalid UserName");
                // Frontendu ce biti poslato StatusCode=401 u Response Status Line, a  "Invalid UserName" u Response Body.

            // Ako UserName dobar, proverava password tj hashes it and compares it with PasswordHash column in AspNetUsers 
            var result = await _signInManager.CheckPasswordSignInAsync(appUser, loginDTO.Password, false); 

            if (!result.Succeeded)
                return Unauthorized("Invalid Password"); 
                // Frontendu ce biti poslato StatusCode=401 u Response Status Line, a "Invalid Password" u Response Body.

            return Ok(new NewUserDTO { UserName = appUser.UserName, EmailAddress = appUser.Email, Token = _tokenService.CreateToken(appUser)});
            // Frontendu ce biti poslato StatusCode=200 u Response Status Line, a NewUserDTO u Response Body.
        }

    }
}
