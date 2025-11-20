using Api.DTOs.Account;
using Api.Exceptions_i_Result_pattern;

namespace Api.Interfaces
{   
    // Odgovoran za AccountController endpoints - pogledaj Services.txt 
    public interface IAccountService // Postoji built-in IAuthenticationService, ali necu njega jer ne znam ga, vec pravim svoj custom
    {
        //Servis prima DTO iz kontroler, mapira DTO->Entity, salje Entity u Repository, Repository vraca Entity, servis mapira Entity->DTO i salje DTO kontroleru

        Task<NewUserDTO> RegisterAsync(RegisterDTO registerDTO);
        Task<Result<NewUserDTO>> LoginAsync(LoginDTO loginDTO);
        Task ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDTO);
        Task ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);
        Task<AccessAndRefreshTokenDTO> RefreshTokenAsync(string? refreshToken);
    }
}
