using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Interfaces;
using Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Api.Service
{
    public class TokenService : ITokenService // U Program.cs registrujem da ITokenService se odnosi na TokenService
    {   
        private readonly IConfiguration _configuration; // Za pristup svemu iz appsettings
        // _configuration je isto kao builder.Configuration u Program.cs
        private readonly SymmetricSecurityKey _signingKey; // Symetric jer ocu sa istim kljucem to Sign and Verify JWT. SHA256 moram zbog ovoga koristiti.
        public TokenService(IConfiguration configuration) 
        { 
            _configuration = configuration;
            _signingKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"])); 
        }

        // Generate JWT after new User successfully registers in Register method of AccountController.
        // Generate JWT after existing User successfully logs in in Login method of AccountController. 
        public string CreateToken(AppUser appUser)
        {
            //Claims are better than Roles jer zelim da dohvatim AppUser polja iz Login/Register forme bez da gledam u bazu posto Claims (Email i Username polja iz AppUser u mom slucaju) nisu nikad u bazi, vec in-memory i onda ih brze i lakse dohvatim.
            var claims = new List<Claim>
            {   
                new Claim(JwtRegisteredClaimNames.Email, appUser.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, appUser.UserName)
            };

            // Signing credentials  = type of Encryption
            var signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), // Email i UserName from AppUser
                Expires = DateTime.Now.AddDays(7), // User moze max 7 dana biti logged in, nakon cega FE nece moci da salje Request dok se manualno logout pa opet login da dobije novi JWT
                SigningCredentials = signingCredentials,
                Issuer = _configuration["JWT:Issuer"], // From appsettings
                Audience = _configuration["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token); // Signed JWT string that is sent to Client
        }
    }
}
