using System.Net;
using System.Text.Encodings.Web;
using Api.DTOs.Account;
using Api.Interfaces;
using Api.Models;
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/account")] // https://localhost:port/api/account
    [ApiController]
    public class AccountController : ControllerBase
    {   // Interface za sve klase zbog DI, dok u Program.cs napisem da prepozna interface kao tu klasu
        private readonly UserManager<AppUser> _userManager;     // Ovo moze jer AppUser:IdentityUser 
        private readonly SignInManager<AppUser> _signInManager; // Ovo moze jer AppUser:IdentityUser 
        // Zbog AppUser updating via UserManager i SignInManager, automatski je implementiran Race Condition using ConcurrencyStamp kolonu of IdentityUser
        private readonly ITokenService _tokenService; // U Program.cs definisali da prepozna ITokenService kao TokenService
        private readonly IEmailService _emailSender;  // U Program.cs definisali da prepozna IEmailSender kao EmailService
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IEmailService emailService, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailSender = emailService;
            _logger = logger;
        }

        /* Svaki Endpoint:
            - koristi DTO kao argumente i DTO za slanje objekata to FE, jer dobra praksa je ne dirati Models (Entity) klase (koje predstavljaju tabele u bazi) koje su namenjene za Repository tj EF Core.
            - bice tipa Task<IActionResult<T>> jer IActionResult<T> omoguci return of StatusCode + Data of type T, dok Task omogucava async. 
            - salje to FE Response koji ima polja Status, Headers i Body. 
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

         Ako endpoint nema [Authorize] ili User.GetUserName(), FE ne treba slati JWT in Request Header, ali ako ima bar 1 od ova 2, onda treba.
         
         Ne koristim CancellationToken jer neam pure async endpoint (iako pise async), zato sto await metode u njima ne prihvataju CancellationToken jer nisu custom, vec built-in tipa koji ne prihvata CancellationToken jer nema potrebe za tim. Mogu da im uradim extension, ali nema poente.
         
         Rate Limiter objasnjen u Program.cs
         
         Race conditions regarding AppUser je automatski odradjen u Register/Login/ResetPassword/RefreshToken endpoint, jer IdentityUser sadrzi ConcurrencyStamp polje koje se automatski menja svaki put kad azuriramo usera (vrstu AspNetUsers tabele) tako sto
        _userManage.UpdateAsync/ResetPasswordAsync automatski zameni ConcurrencyStamp kolonu i sprecava da se u "isto vreme" zameni neko user polje - pogledaj Race Conditions.txt
         
         Race conditions regarding Refresh Token in RefreshToken endpoint je sredjen rucno - pogledaj Race Conditions.txt

         Access Token and Refresh Token objasnjeni u SPA Security Best Practice.txt 
         */

        //[EnableRateLimiting("fast")] - nesto nije htelo kad sam imao ovaj ratelimiter ukljucen
        [HttpPost("register")] // https://localhost:port/api/account/register
        // Ne ide [Authorize] jer ovo je Register, posto ovaj endpoint mora biti svima dostupan
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {   /* Pre ove metode, pokrenuto je OnModelCreating iz ApplicationDBContext i napunjena je AspNetRoles tabela prilikom Migracije (ako sam uradio migraciju uopste).
               
               Kad novi User ukuca Email i Password, AspNetCoreIdentity ga upise u bazi kroz ApplicationDbContext : IdentityDbContext<AppUser> tj
            pretrazuje AspNetUsers tabelu i poredi uneti password sa stvarnim u toj tabeli. Moze da return i podatke iz Identity tabela ako treba. 
            Tek nakon ovoga, poziva se CreateToken metoda iz TokenService da se JWT generise, jer JWT treba kako ne bi, za svaki API request made in FE, BE gledao u DB da proveri usera, ali provera usera 
            je dovoljna prilikom Register/Login endpoint, a nakon toga ne zelimo, jer nema potrebe.
            
               Kad gadjam iz ReactTS FE ovaj endpoint, moram polja da nazovem i prosledim redosledom kao u RegisterDTO jer RegisterDTO je tip input argumenta, a zbog [FromBody] moram u body of POST Request ih staviti.
             */

            // Try-Catch, jer cesto se desava server error when using UserManager, a to je runtime error, pa obzirom da nista u try ne baca gresku cak ni implicitno stavi
            try
            {
                // ModelState pokrene validation za RegisterDTO tj za zeljena RegisterDTO polja proverava na osnovu njihovih annotations.
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                    // Frontendu ce biti poslato StatusCode=400 u Response Status Lie, a ModelState objekat bice poslat u Response Body sa RegisterDTO poljima (EmailAddress, UserName i Password) u "errors" delu

                var appUser = new AppUser
                {
                    UserName = registerDTO.UserName,
                    Email = registerDTO.EmailAddress
                };

                var createdUser = await _userManager.CreateAsync(appUser, registerDTO.Password); // Dodaje novog usera u AspNetUsers (IdentityUser) tabelu 
                /* Dodaje AppUser polja (UserName i Email) i register.Password (koga automatski hash-uje) u AspNetUsers tabelu tj kreira novi User u toj tabeli
                   CreateAsync sprecava kreiranje 2 usera sa istim UserName ili Email(za email sam morao u Program.cs da definisem rucno u AddIdentity)
                   CreateAsync behing the scenes attaches appUser to EF Core and populates every column of AspNetUsers table regarding ConcurrencyStamp column koja sprecava race conditions
                 */
                if (createdUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(appUser, "User"); // Mogo sam samo User ili Admin upisati za Role, jer samo te vrednosti su seedovane migracijom u OnModelCreating u AspNetRoles tabelu
                    // Dodaje u AspNetUserRoles tabelu koja automatski ima RoleId FK koji gadja Id u AspNetRoles i UserId FK koji gadja Id u AspNetUsers tabeli 

                    if (roleResult.Succeeded)
                    {
                        var accessToken = _tokenService.CreateToken(appUser);
                        var refreshToken = _tokenService.GenerateRefreshToken(); 
                        var hashedRefreshToken = _tokenService.HashRefreshToken(refreshToken); // U tabelu samo hashovan token stavljam

                        appUser.RefreshTokenHash = hashedRefreshToken; // U DB ide hash, dok u Cookie ide non-hashed refresh token
                        appUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Refresh token is long lived 
                        appUser.LastRefreshTokenUsedAt = new DateTime(2000, 1, 1); // Prilikom register, user nije nijednom jos uvek koristio Refresh Token, pa mu dodajem neku idiotsku vrednost koja simulira nikad korisceno
                        await _userManager.UpdateAsync(appUser); // ConcurrencyStamp column of IdentityUser prevents overwriting if another request wanna update the same user right after registration => race condition prevented

                        // Refresh Token (not hashed !) is sent to Browser(not to FE) via highly-secured Cookie to prevent CSRF attack
                        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true, // Mora zbog SameSite=None, ali FE mora u HTTPS
                            SameSite = SameSiteMode.None,
                            Expires = DateTime.UtcNow.AddDays(7), // Mora isto kao appUser.RefreshTokenExpiryTime
                            Path = "/", // Allow cookie to be sent to all endpoints, not just to refresh-token endpoint. If FE axios instance tries to send the refresh request from an interceptor that was triggered by a different protected endpoint (npr /api/stocks), the browser might not send the cookie
                            IsEssential = true, // Ne kapiram zasto ali ovo nekad mora.
                        });

                        return Ok(new NewUserDTO { UserName = appUser.UserName, EmailAddress = appUser.Email, Token = accessToken });
                    }
                    // Frontendu ce biti poslato NewUserDTO u Response Body, a StatusCode=200 u Response Status Line.

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
            {   // Iako nigde u try block nema explicitni throw, niti implicitni throw, catch block sluzi zbog runtime unexpected errors.

                return StatusCode(500, ex);
                // Frontendu ce biti poslato StatusCode=500 u Response Status Line, a exception u Response Body.
            }
        }

        [HttpPost("login")] // https://localhost:port/api/account/login
        // Ne ide [Authorize] jer je ovo Login
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {   /* Kad existing User ukuca Email i Password, AspNetCoreIdentity ga nadje u bazi kroz ApplicationDbContext : IdentityDbContext<AppUser> tj
            pretrazuje AspNetUsers tabelu i poredi uneti password sa stvarnim u toj tabeli. Moze da return i podatke iz Identity tabela ako treba. 
            Tek nakon ovoga, poziva se CreateToken metoda iz TokenService da se JWT generise, jer JWT treba kako ne bi, za svaki API request, app gledao u DB kao prilikom user login sto uradi.
            
            Kad pozivam iz ReactTS Frontenda ovaj Endpoint, moram polja da nazovem i prosledim redosledom kao u LoginDTO jer LoginDTO je tip input argumenta. Zbog [FromBody] moram u body of Request ih staviti.
            */

            // ModelState pokrene validaiton iz LoginDTO tj za zeljena LoginDTO polja proverava na osnovu njihovih annotations.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a ModelState objekat bice poslat u Response Body sa LoginDTO poljima (UserName i Password) u "errors" delu poruke

            var appUser = await _userManager.FindByNameAsync(loginDTO.UserName); // Bolji i sigurniji nacin nego 2 linije iznad i takodje pretrazuje AspNetUsers tabelu da nadjemo AppUser by UserName
            // appUser je ocitao sve kolone iz zeljene vrste AspNetUsers tabele, medju kojima je i ConcurrencyStamp koji sprecava race conditions

            if (appUser is null)
                return Unauthorized("Invalid UserName");
                // Frontendu ce biti poslato StatusCode=401 u Response Status Line, a "Invalid UserName" u Response Body.

            // Ako UserName dobar, proverava password tj hashes it and compares it with PasswordHash column in AspNetUsers jer ne postoji Password kolona u AspNetUsers vec samo PasswordHash zbog sigurnosti
            var result = await _signInManager.CheckPasswordSignInAsync(appUser, loginDTO.Password, false);

            if (!result.Succeeded)
                return Unauthorized("Invalid Password");
                // Frontendu ce biti poslato StatusCode=401 u Response Status Line, a "Invalid Password" u Response Body.

            var accessToken = _tokenService.CreateToken(appUser);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var hashedRefreshToken = _tokenService.HashRefreshToken(refreshToken); // Before putting it in DB dobro je hashovati ga

            appUser.RefreshTokenHash = hashedRefreshToken; // U DB ide hash, dok u Cookie ide obican refresh token
            appUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Refresh token is long lived. 
            appUser.LastRefreshTokenUsedAt = new DateTime(2000, 1, 1); // Prilikom login, user nije nijednom jos uvek koristio Refresh Token, pa mu dodajem neku idiotsku vrednost koja simulira nikad korisceno
            await _userManager.UpdateAsync(appUser); // ConcurrencyStamp column of IdentityUser stops a race where two logins for the same account try to update refresh token fields at the same time - race condition sprecen. 

            // Refresh Token (not hashed !) is sent to Browser(not to FE) via highly-secured Cookie to prevent CSRF attack
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Mora zbog SameSite=None, ali FE mora u HTTPS
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7), // Mora da odgovara appUser.RefreshTokenExpiryTime
                Path = "/", // Allow cookie to be sent to all endpoints, not just refresh-token. if axios instance tries to send the refresh request from an interceptor that was triggered by a different protected endpoint (npr /api/stocks), the browser might not send the cookie
                IsEssential = true, // Ne kapiram zasto ali ovo nekad mora.
            });

            return Ok(new NewUserDTO { UserName = appUser.UserName, EmailAddress = appUser.Email, Token = accessToken });
            // Frontendu ce biti poslato StatusCode=200 u Response Status Line, a NewUserDTO u Response Body.
        }

        [HttpPost("forgotpassword")] // https://localhost:port/api/account/forgotpassword
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDTO)
        // Email se obicno salje u Request Body from FE, moze i FromQuery, ali nije dobra praksa
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a polje EmailAddress iz ModelState tj iz ForgotPasswordDTO ce biti prosledjeno u "errors" delu of Request sa objasnjenjem - u handleError smo uhvatili ovaj tip greske 

            // Ako user namerno unese email koji nije u bazi, BE vraca OK("Reset password link is sent to your email") zbog sigurnosti jer onda je user ustvari napadac i da ne provali da taj mejl ne postoji pa da ne predje na drugi email koji mozda postoji. Ovako zbunimo napadaca.

            var user = await _userManager.FindByEmailAsync(forgotPasswordDTO.EmailAddress);
            // If email is not found, just send "success" mesage to FE da zavaramo trag napadacu.
            // user sadrzi celu vrstu iz AspNetUsers tabele medju kojom je ConcurrencyStamp kolona koja sprecava race conditions 

            if (user is null)
                return Ok("Reset password link is sent to your email");
                // Frontendu ce biti poslato StatusCode=200 u Response Status Line, a "Reset password link is sent to your email" in Body.

            // If email found, generate a secure & time-limited (by default to 1day) reset token, send email and success message to FE
            var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user); // token je samo za ovog usera validan i nije skladisten u bazi 
            // Encode token to safely include in URL later
            var encodedToken = WebUtility.UrlEncode(passwordResetToken); // Encode before putting in URL
            // Create frontend reset-password url 
            string frontendBaseUrl = Env.GetString("frontendBaseURL"); // U Program.cs sam Env.Load() i zato ovo moze.
            var resetUrl = $"{frontendBaseUrl}/reset-password?token={encodedToken}&email={user.Email}"; // Ne treba email biit poslat, rizicno je !
            // frontendBaseUrl/reset-password mora da postoji u FE kao route da bi ovo moglo !
            // Send email to user.Email containing "Reset Password" subject and message containing reset-password url 
            await _emailSender.SendEmailAsync(user.Email, "Reset Password", $"Click <a href='{HtmlEncoder.Default.Encode(resetUrl)}'>here</a> to reset your password.");
            // URL je oblika http://localhost:port/reset-password?token=sad8282s9&email=adresa@gmail.com. Iako imam ovaj Query Parameter, ReactTS svakako otvara http://localhost:port/reset-password (ResetPasswordPage)         
            
            // Kada .NET sends rest password url, odma zaboravi kakav je token, jer token nije skladisten nigde. Onda u ResetPasswrod FE posalje taj token ,ali .NET ima mehanizam, u ResetPasswordAsync, koji decodes token i vidi user credentials u tokenu
            
            // Send success message to FE 
            return Ok("Reset password link is sent to your email"); 
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                // Frontendu ce biti poslato StatusCode=400 u Response Status Line, a polje EmailAddress iz ModelState tj iz ResetPasswordDTO ce biti prosledjeno u "errors" delu of Request sa objasnjenjem - u handleError smo uhvatili ovaj tip greske 

            // Find user by email
            var user = await _userManager.FindByEmailAsync(resetPasswordDTO.EmailAddress); // Ocitane sve kolone ove vrste, pa i ConcurrencyStamp koja sprecava race conditions

            // Always return success to prevent email enumeration attack ! 
            // But only actually reset if user exists
            if (user is not null)
            {
                _logger.LogInformation("User found: {Email}", resetPasswordDTO.EmailAddress);
                var result = await _userManager.ResetPasswordAsync(user, resetPasswordDTO.ResetPasswordToken, resetPasswordDTO.NewPassword);
                /* ResetPasswordAsync ima mehanizam da decodes token i da izvadi sve iz njega i provedi da li je to isto kao kad je ForgotPassword endpoint encodovao token.
                 Proveri li je za ovaj user generisan resetPasswordToken u ForgotPassword endpoint i da li NewPassword se slaze sa zahtevima u Program.cs
                 Due to ConcurrencyStamp column of IdentityUser, ResetPasswordAsync azurira tu kolonu cime prevents overwriting if another request has updated the user since the reset token was issued - race condition sprecen
                */
                if (!result.Succeeded)
                {
                    _logger.LogError("Password reset failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

                    return Ok("If the email exists in our system, the password has been reset"); // Iako nije uspeo, foliram da jeste da napadaca ometem.
                }
                else
                {
                    _logger.LogInformation("Password reset successful for {Email}", resetPasswordDTO.EmailAddress);
                }
            }
            else
            {
                _logger.LogWarning("User not found: {Email}", resetPasswordDTO.EmailAddress);
            }

            // Always return the same response regardless of whether user exists or reset succeeded
            // This prevents timing attacks and email enumeration
            return Ok("If the email exists in our system, the password has been reset.");
        }

        /* Kada FE posalje Request to protected endpoint, FE automatski odredi da li je trenutni JWT(Access Token) blizu isteka, pa ako jeste, automatski kaze Browseru da preko Cookie (koji sadrzi Refresh Token) pozove ovaj endpoint 
         da BE, za tog usera, napravi novi Refresh Token, invalidira stari i kreira novi JWT. Nema argumenta, jer Browser(ne FE) mu salje Cookie.*/
        [HttpGet("refresh-token")]
        //[EnableRateLimiting("slow")] // To prevent attacks
        public async Task<IActionResult> RefreshToken()
        {   
            // Access Cookies sent by the Browser( when client wanted it). Isti ovaj Cookie je Login/Register Endpoint poslao Browseru
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken)) // U Login/Register sam nazvao refreshToken i zato ovde ga ocitavam
                return Unauthorized("No refresh token provided");
                
            _logger.LogInformation("BE primio refreshtoken cookie");
                
            var hashedRefreshToken = _tokenService.HashRefreshToken(refreshToken); // Jer u bazi skladistim Hash Refresh Token, dok u Cookie je obican Refresh Token
            
            var appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == hashedRefreshToken);

            if (appUser is null || appUser.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized("Invalid refresh token");

            // Prevent double use:  poredim sa DateTime(2000,1,1) jer je to za LastRefreshTokenUsedAt u Login/Register postavljeno kao inicijalna vrednost oznavajuci da RefreshToken nije koriscen ni jednom do tada
            if (appUser.LastRefreshTokenUsedAt > new DateTime(2000, 1, 1) && (DateTime.UtcNow - appUser.LastRefreshTokenUsedAt).TotalSeconds < 10)
            {   // Uslov je 10s za real-world apps koji osigurava da ne moze unutar 10s 2 ili vise puta da ovaj endpoint bude pozvan. Ovo je u skladu sa 30s JWT expiry time u AxiosWithJWTForBackend.tsx u FE, jer ako nije, onda problem
                return Unauthorized("Refresh token used too frequently");
            }

            // Token rotation
            var newAccessToken = _tokenService.CreateToken(appUser);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var newHashedRefreshToken = _tokenService.HashRefreshToken(newRefreshToken);

            appUser.RefreshTokenHash = newHashedRefreshToken;
            appUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            appUser.LastRefreshTokenUsedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(appUser); // Prevents a race condition, jer azurira automatski ConcurrencyStamp kolonu, wheen two refresh requests try to update refresh token field simultaneously.

            // Salje secured Cookie to Browser koji mora imati isti key "refreshToken" kao Cookie poslat iz Login/Register kako bi na pocetku ovog Endpoint mogo uvek da dohvatim refresh token nebitno da l je novi ili inicijalni
            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Mora zbog SameSite=None, ali FE mora u HTTPS
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7), // Mora ito kao appUser.RefreshTokenExpiryTime
                Path = "/", // Allow cookie to be sent to all endpoints, not just refresh-token. if axios instance tries to send the refresh request from an interceptor that was triggered by a different protected endpoint (npr /api/stocks), the browser might not send the cookie
                IsEssential = true, // Ne kapiram zasto ali ovo nekad mora.
            });

            return Ok( new { accessToken = newAccessToken } ); // Access Token se salje kroz Response Body
        }

    }
}
