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

        // Svaki Endpoint bice tipa IActionResult jer on omogucava return of HTTP Status + Data 

        [HttpPost("register")] // // https://localhost:port/api/account/register
        // Ne ide [Authenticate] jer ovo je Register 
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {   /* Pre ove metode , pokrenuto je OnModelCreating iz ApplicationDBContext i napunjena je AspNetRoles tabela prilikom Migracije (ako sam migrirao).
               
               Kad novi User ukuca Email i Password, poziva se AspNetCoreIdentity da ga nadje u bazi kroz ApplicationDbContext : IdentityDbContext<AppUser> tj
            pretrazuje AspNetUsers tabelu i poredi uneti password sa stvarnim u toj tabeli. Moze da returnuje i podatke iz AspNetUserRoles i AspNetRoles i AspNetClaims tabela ako treba. 
            Tek nakon ovoga, poziva se CreateToken metoda iz TokenService da se JWT generise, jer JWT treba kako ne bi, za svaki API request, app gledao u DB kao prilikom user Registering sto uradi.  
            
               Kad gadjam iz ReactTS Frontenda ovaj Endpoint, moram polja da nazovem i prosledim redosledom kao u RegisterDTO jer RegisterDTO je tip input argumenta. 
             */


            // Try-Catch, jer cesto se desava server error when using UserManager
            try
            {   
                // Pokrene Data Validaiton iz RegisterDTO
                if(!ModelState.IsValid)
                    return BadRequest(ModelState);

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
                    else
                        return StatusCode(500, roleResult.Errors);
                }
                else // Ako vec postoji user sa istim EmailAddres ili UserName
                {
                    return StatusCode(500, createdUser.Errors);
                }

            } catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPost("login")] // https://localhost:port/api/account/login
        // Ne ide [Authorize] jer je ovo Login
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {   /* Kad existing User ukuca Email i Password, poziva se AspNetCoreIdentity da ga nadje u bazi kroz ApplicationDbContext : IdentityDbContext<AppUser> tj
            pretrazuje AspNetUsers tabelu i poredi uneti password sa stvarnim u toj tabeli. Moze da returnuje i podatke iz AspNetUserRoles i AspNetRoles i AspNetClaims tabela ako treba. 
            Tek nakon ovoga, poziva se CreateToken metoda iz TokenService da se JWT generise, jer JWT treba kako ne bi, za svaki API request, app gledao u DB kao prilikom user login sto uradi.
            
            Kad gadjam iz ReactTS Frontenda ovaj Endpoint, moram polja da nazovem i prosledim redosledom kao u LoginDTO jer LoginDTO je tip input argumenta. 
            */

            // Pokrene Data Validaiton iz LoginDTO
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginDTO.UserName.ToLower()); // Moze i FindAsync jer je brze 
            // _userManager.Users odnsosi se na AspNetUsers tabelu
            var appUser = await _userManager.FindByNameAsync(loginDTO.UserName); // Bolji i sigurniji nacin nego linija iznad i takodje pretrazuje AspNetUsers tabelu

            if (appUser is null)
                return Unauthorized("Invalid UserName");

            // Ako UserName dobar, proverava password tj hashes it and compares it with PasswordHash column in AspNetUsers 
            var result = await _signInManager.CheckPasswordSignInAsync(appUser, loginDTO.Password, false); 

            if (!result.Succeeded)
                return Unauthorized("Invalid Password"); 

            return Ok(new NewUserDTO { UserName = appUser.UserName, EmailAddress = appUser.Email, Token = _tokenService.CreateToken(appUser)});
        }

    }
}
