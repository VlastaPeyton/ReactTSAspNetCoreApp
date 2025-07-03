using Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Interfaces
{
    // Za svaku klasu koja predstavlja Service pravim interface pomocu koga radim DI u Controller, dok u Program.cs pisem da prepozna interface kao zeljenu klasu

    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
