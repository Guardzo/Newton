using GuradzoApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GuradzoApi.Services
{
    public class UserService
    {
        private readonly GuradoDbContext _context;
        private readonly PasswordHasher<User> _hasher = new();

        public UserService(GuradoDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return false;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task CreateUserAsync(string username, string password)
        {
            if (await UserExistsAsync(username)) throw new Exception("User already exists");

            var user = new User
            {
                Username = username,
                PasswordHash = _hasher.HashPassword(null!, password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ResetPasswordAsync(string username, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return false;

            user.PasswordHash = _hasher.HashPassword(user, newPassword);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
