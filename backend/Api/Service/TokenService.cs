using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Api.Interfaces;
using Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Api.Service
{   
    /* Metode su sync, jer nema potrebe da budu async posto metode ne pricaju sa bazom
       
       Creating Access Token(short-lived) tj CreateToken method, je stateless tj JWT se ne unosi u bazu za korisnika, zato sto user claims are encoded in JWT. 
       Creating Refres Token(long-lived) tj GenerateRefreshToken method, nije stateless tj Refres Token se unosi u bazu za korisnika. 
       HashRefreshTOken jer before persisting Refresh Token in DB for corresponding user, needs to be hashed. 
     */
    public class TokenService : ITokenService // U Program.cs registrujem da ITokenService se odnosi na TokenService
    {   
        private readonly IConfiguration _configuration; // Za pristup svemu iz appsettings. _configuration je isto kao builder.Configuration u Program.cs
        private readonly SymmetricSecurityKey _signingKey; // Symetric jer ocu sa istim kljucem to Sign and Verify JWT. SHA256 moram zbog ovoga koristiti.
        public TokenService(IConfiguration configuration) 
        { 
            _configuration = configuration;
            _signingKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"])); 
        }

        // Generate JWT(short lived access token) after new User successfully registers in Register method of AccountController.
        // Generate JWT after existing User successfully logs in in Login method of AccountController. 
        // Access Token is stateless tj ne upisuje se u bazu za zeljenog usera jer user claims are encoded in JWT.
        public string CreateToken(AppUser appUser)
        {
            //Claims are better than Roles jer zelim da dohvatim AppUser polja iz Login/Register forme bez da gledam u bazu posto Claims (Email i Username polja iz AppUser u mom slucaju) nisu nikad u bazi, vec in-memory i onda ih brze i lakse dohvatim.
            var claims = new List<Claim>
            {   
                new Claim(JwtRegisteredClaimNames.Email, appUser.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, appUser.UserName)
                // U ClaimsExtension.cs samo mogu dohvatiti Email ili GivenName jer samo sam njih ovde setovao
            };

            // Signing credentials  = type of Encryption
            var signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), // Email i UserName from AppUser
                Expires = DateTime.UtcNow.AddMinutes(15), // Objasnjeno u "SPA Security Best Practice.txt"
                SigningCredentials = signingCredentials,
                Issuer = _configuration["JWT:Issuer"], // From appsettings
                Audience = _configuration["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token); // Signed JWT string that is sent to Client
        }
        
        // Ne moze Generate Refresh Token u CreateToken, jer ovo je stateless tj upis u bazu mora
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            } 
            // Automatski close resources zbog using
        }
        // Before
        public string HashRefreshToken(string refreshToken)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
