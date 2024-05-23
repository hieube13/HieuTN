using AdminApp.Models;
using IdentityModel.Client;

namespace AdminApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponse> LoginAsync(LoginRequest loginRequest); 
        Task LogoutAsync();
    }
}
