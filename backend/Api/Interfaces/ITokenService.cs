using Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Interfaces
{
    // Za svaku klasu koja predstavlja Service pravim interface pomocu koga radim DI u Controller, dok u Program.cs pisem da prepozna interface kao zeljenu klasu

    public interface ITokenService
    {   
        // Nema potrebe za async, jer ovo je obicno sync, jer nijedna metoda ne komunicira sa bazom npr pa da se ceka neko izvrsenje, vec su ovo brze metode
        string CreateAccessToken(AppUser user);
        string CreateRefreshToken();
        string HashRefreshToken(string refreshToken);
    }
}
