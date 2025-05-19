using System.Security.Claims;

namespace Api.Extensions
{
    public static class ClaimsExtensions
    {
        public static string GetUserName(this ClaimsPrincipal user)
        {   // ClaimsPrincipal je AppUser
            // Claims je Email i UserName(GivenName) (tako pise u TokenService)
            return user.Claims.SingleOrDefault(x => x.Type == ClaimTypes.GivenName).Value; // UserName izvuce from AppUser
        }
    }
}
