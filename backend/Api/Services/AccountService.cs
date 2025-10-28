using System.Net;
using System.Text.Encodings.Web;
using Api.DTOs.Account;
using Api.Exceptions;
using Api.Exceptions_i_Result_pattern;
using Api.Interfaces;
using Api.Models;
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    // Pogledaj Services.txt
    public class AccountService : IAccountService
    {
        // Interface za sve klase zbog DI, dok u Program.cs registrujem da prepozna interface kao tu klasu + zbog testabilnosti - pogledaj Dependency Injection.txt
        private readonly UserManager<AppUser> _userManager;     // Ovo moze jer AppUser:IdentityUser 
        private readonly SignInManager<AppUser> _signInManager; // Ovo moze jer AppUser:IdentityUser 
        // Zbog AppUser updating via UserManager i SignInManager, automatski je implementiran Race Condition using ConcurrencyStamp kolonu of IdentityUser
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService; 
        private readonly ILogger<AccountService> _logger;

        public AccountService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        // Metode nemaju try-catch - pogledaj Services.txt
        public async Task<NewUserDTO> RegisterAsync(RegisterDTO registerDTO)
        {
            
            AppUser appUser = new AppUser
            {
                UserName = registerDTO.UserName,
                Email = registerDTO.EmailAddress
            };

            var createdUser = await _userManager.CreateAsync(appUser, registerDTO.Password!); // Dodaje novog usera u AspNetUsers (IdentityUser) tabelu 
            /* Dodaje AppUser polja (UserName i Email) i registerDTO.Password (koga automatski hash-uje) u AspNetUsers tabelu tj kreira novi User u toj tabeli
                CreateAsync sprecava kreiranje 2 usera sa istim UserName ili Email(za email sam morao u Program.cs da definisem rucno u AddIdentity)
                CreateAsync behing the scenes attaches appUser to EF Core and populates every column of AspNetUsers table regarding ConcurrencyStamp column koja sprecava race conditions
            */
            if (!createdUser.Succeeded)
                throw new UserCreatedException($"User creation failed in _userManager: {createdUser.Errors}"); 
            

            var roleResult = await _userManager.AddToRoleAsync(appUser, "User"); // Mogu samo User ili Admin upisati za Role, jer samo te vrednosti su seedovane migracijom u OnModelCreating u AspNetRoles tabelu
            if (!roleResult.Succeeded)   
                throw new RoleAssignmentException($"Role assignment failed in _userManager: {roleResult.Errors}");  
            

            var accessToken = _tokenService.CreateAccessToken(appUser);
            var refreshToken = _tokenService.CreateRefreshToken();
            var hashedRefreshToken = _tokenService.HashRefreshToken(refreshToken); // Hash, jer u DB samo hash refresh token stavljam

            appUser.RefreshTokenHash = hashedRefreshToken; // U DB ide hash, dok u Cookie ide non-hashed refresh token
            appUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Refresh token is long lived 
            appUser.LastRefreshTokenUsedAt = new DateTime(2000, 1, 1); // Prilikom register, user nije nijednom jos uvek koristio Refresh Token, pa mu dodajem neku idiotsku vrednost koja simulira nikad korisceno
            // Moram azurirati appUser u bazi zbog RefreshToken
            await _userManager.UpdateAsync(appUser); // ConcurrencyStamp column of IdentityUser prevents overwriting if another request wanna update the same user right after registration => race condition prevented

            // Service ne radi nista sa Cookies (refreshToken), vec to ostaje odgovornost of AccountController. Service samo prosledi not-hashed refreshToken, jer to ide u Cookie.
            return new NewUserDTO { UserName = appUser.UserName, EmailAddress = appUser.Email, Token = accessToken, RefreshToken = refreshToken};  
        }

        public async Task<Result<NewUserDTO>> LoginAsync(LoginDTO loginDTO)
        {   
            
            var appUser = await _userManager.FindByNameAsync(loginDTO.UserName); // Bolji i sigurniji nacin nego 2 linije iznad i takodje pretrazuje AspNetUsers tabelu da nadjemo AppUser by UserName
                                                                                    // appUser je ocitao sve kolone iz zeljene vrste AspNetUsers tabele, medju kojima je i ConcurrencyStamp koji sprecava race conditions
            if (appUser is null)
                //throw new WrongUsernameException($"Invalid username"); - postalo Result pattern jer nije neocekivana greska systema, vec biznis logika
                return Result<NewUserDTO>.Fail("Invalid username");

            // Ako UserName dobar, proverava password tj hashes it and compares it with PasswordHash column in AspNetUsers jer ne postoji Password kolona u AspNetUsers vec samo PasswordHash zbog sigurnosti
            var result = await _signInManager.CheckPasswordSignInAsync(appUser, loginDTO.Password, false);
            if (!result.Succeeded)
                //throw new WrongPasswordException($"Invalid password"); - postalo Result pattern jer nije neocekivana greska systema, vec biznis logika
                return Result<NewUserDTO>.Fail("Invalid password");

            var accessToken = _tokenService.CreateAccessToken(appUser);
            var refreshToken = _tokenService.CreateRefreshToken();
            var hashedRefreshToken = _tokenService.HashRefreshToken(refreshToken); // Before putting it in DB dobro je hashovati ga

            appUser.RefreshTokenHash = hashedRefreshToken; // U DB ide hash, dok u Cookie ide obican refresh token
            appUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Refresh token is long lived. 
            appUser.LastRefreshTokenUsedAt = new DateTime(2000, 1, 1); // Prilikom login, user nije nijednom jos uvek koristio Refresh Token, pa mu dodajem neku idiotsku vrednost koja simulira nikad korisceno
            // Moram azurirati appUser u bazi zbog Refresh Token
            await _userManager.UpdateAsync(appUser); // ConcurrencyStamp column of IdentityUser stops a race where two logins for the same account try to update refresh token fields at the same time - race condition sprecen. 

            // Service ne radi nista sa Cookies (refreshToken), vec to ostaje odgovornost of AccountController. Service samo prosledi not-hashed refreshToken, jer to ide u Cookie.
            var newUserDTO = new NewUserDTO { UserName = appUser.UserName, EmailAddress = appUser.Email, Token = accessToken, RefreshToken = refreshToken };

            return Result<NewUserDTO>.Success(newUserDTO);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDTO)
        {   
            var user = await _userManager.FindByEmailAsync(forgotPasswordDTO.EmailAddress);
            // If email is not found, just send "success" mesage to FE da zavaramo trag napadacu.
            // user sadrzi celu vrstu iz AspNetUsers tabele medju kojom je ConcurrencyStamp kolona koja sprecava race conditions 
            if (user is null)          
                throw new ForgotPasswordException("Reset password link is sent to your email ali ne znas da l je uspesno ili nije jer fora je da zavaram trag");
            
            // If email found, generate a secure & time-limited (by default to 1day) reset token, send email and success message to FE
            var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user); // token je samo za ovog usera validan i nije skladisten u bazi 
            // Encode token to safely include in URL later
            var encodedToken = WebUtility.UrlEncode(passwordResetToken); // Encode before putting in URL
            // Create frontend reset-password url 
            string frontendBaseUrl = Env.GetString("frontendBaseURL"); // U Program.cs sam Env.Load() i zato ovo moze.
            var resetUrl = $"{frontendBaseUrl}/reset-password?token={encodedToken}&email={user.Email}"; // Ne treba email biit poslat, rizicno je !
            // frontendBaseUrl/reset-password mora da postoji u FE kao route da bi ovo moglo !
            // Send email to user.Email containing "Reset Password" subject and message containing reset-password url 
            await _emailService.SendEmailAsync(user.Email, "Reset Password", $"Click <a href='{HtmlEncoder.Default.Encode(resetUrl)}'>here</a> to reset your password.");
            // URL je oblika http://localhost:port/reset-password?token=sad8282s9&email=adresa@gmail.com. Iako imam ovaj Query Parameter, ReactTS svakako otvara ResetPassword endpoint tj http://localhost:port/reset-password (ResetPasswordPage)         
            // Kada .NET sends reset password url, odma zaboravi kakav je token, jer token nije skladisten nigde osim u varijabli. Onda user klikne na link u mejlu, cime aktivira ResetPassword endpoint, a .NET ima mehanizam u ResetPasswordAsync, koji decodes token iz linka i vidi user credentials u tokenu
        }

        public async Task ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
        {   
            
            // Find user by email
            var user = await _userManager.FindByEmailAsync(resetPasswordDTO.EmailAddress); // Ocitane sve kolone ove vrste, pa i ConcurrencyStamp koja sprecava race conditions
            if (user is null)
            {
                _logger.LogError("User not found during password reset ali posalji 200 klijentu da zavara trag");
                throw new ResetPasswordException("If the email exists in our system, the password has been reset");
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDTO.ResetPasswordToken, resetPasswordDTO.NewPassword);
            /* Kada user kliknuo Forgot Password, dobio je reset password link u email, a kad kliknuo na link on pokrenuo je ovaj endpoint.
                ResetPasswordAsync ima mehanizam da decodes token i da izvadi sve iz njega i provedi da li je to isto kao kad je ForgotPassword endpoint encodovao token.
                Proveri da l je za ovaj user generisan resetPasswordToken u ForgotPassword endpoint i da li NewPassword se slaze sa zahtevima u Program.cs
                Due to ConcurrencyStamp column of IdentityUser, ResetPasswordAsync azurira tu kolonu cime prevents overwriting if another request has updated the user since the reset token was issued - race condition sprecen
            */
            if (!result.Succeeded)
            {
                _logger.LogError("Password reset failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new ResetPasswordException("If the email exists in our system, the password has been reset");
            }
        }

        public async Task<AccessAndRefreshTokenDTO> RefreshTokenAsync(string? refreshToken)
        {   
            _logger.LogInformation("BE primio refreshtoken cookie");

            var hashedRefreshToken = _tokenService.HashRefreshToken(refreshToken); // Jer u bazi skladistim Hash Refresh Token, dok u Cookie je obican Refresh Token

            var appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == hashedRefreshToken);

            if (appUser is null || appUser.RefreshTokenExpiryTime <= DateTime.UtcNow) 
                throw new RefreshTokenException("User not found during refreshtoken or invalid refreshToken");
            
            // Prevent double use:  poredim sa DateTime(2000,1,1) jer je to za LastRefreshTokenUsedAt u Login/Register postavljeno kao inicijalna vrednost oznavajuci da RefreshToken nije koriscen ni jednom do tada
            if (appUser.LastRefreshTokenUsedAt > new DateTime(2000, 1, 1) && (DateTime.UtcNow - appUser.LastRefreshTokenUsedAt)?.TotalSeconds < 10)
            {   /* Uslov je 10s za real-world apps koji osigurava da ne moze unutar 10s 2 ili vise puta da ovaj endpoint bude pozvan. Sprecavam abuse ovim. 
                    Ovo je u skladu sa 30s JWT expiry time u AxiosWithJWTForBackend.tsx u FE, jer ako nije, onda problem. */
                throw new RefreshTokenException("RefreshToken used too frequentyl");
            }

            // Token rotation
            var newAccessToken = _tokenService.CreateAccessToken(appUser);
            var newRefreshToken = _tokenService.CreateRefreshToken();
            var newHashedRefreshToken = _tokenService.HashRefreshToken(newRefreshToken);

            appUser.RefreshTokenHash = newHashedRefreshToken;
            appUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            appUser.LastRefreshTokenUsedAt = DateTime.UtcNow;
            // Update appUser in DB
            await _userManager.UpdateAsync(appUser); // Prevents a race condition, jer azurira automatski ConcurrencyStamp kolonu, wheen two refresh requests try to update refresh token field simultaneously.

            return new AccessAndRefreshTokenDTO { AccessToken = newAccessToken, RefreshToken = newRefreshToken };
        }
    }
}
