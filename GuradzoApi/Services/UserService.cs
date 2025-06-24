using GuradzoApi.Data;
using GuradzoApi.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GuradzoApi.Services
{
    public class UserService : IUserService
    {
        private readonly GuradoDbContext _context;
        private readonly PasswordHasher<User> _hasher = new();
        private readonly ILoggerService _logger;

        public UserService(GuradoDbContext context, ILoggerService logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            _logger.LogInfo($"Validating user: {username}");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return false;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            _logger.LogInfo($"Check UserExists: {username}");
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> ResetPasswordAsync(string username, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return false;

            user.PasswordHash = _hasher.HashPassword(user, newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task CreateUserAsync(string username, string password, string role = "User")
        {
            _logger.LogInfo($"CreateUser: {username}");
            if (await UserExistsAsync(username)) throw new Exception("User already exists");

            var user = new User
            {
                Username = username,
                PasswordHash = _hasher.HashPassword(null!, password),
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

    }
}
