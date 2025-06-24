using GuradzoApi.Data;

namespace GuradzoApi.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(User user);
        string GenerateRefreshToken();
        Task SaveRefreshTokenAsync(string username, string refreshToken);
        Task<bool> ValidateRefreshTokenAsync(string username, string refreshToken);
    }
}
