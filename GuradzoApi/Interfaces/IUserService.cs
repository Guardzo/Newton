using GuradzoApi.Data;

namespace GuradzoApi.Interfaces
{
    public interface IUserService
    {
        Task<bool> ValidateUserAsync(string username, string password);
        Task<bool> UserExistsAsync(string username);
        Task CreateUserAsync(string username, string password, string role = "User");
        Task<User?> GetUserAsync(string username);
        Task<bool> ResetPasswordAsync(string username, string newPassword);
    }
}
