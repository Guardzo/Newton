using GuradzoApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GuradzoApi.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;
        private readonly GuradoDbContext _context;

        public TokenService(IConfiguration config, GuradoDbContext context)
        {
            _config = config;
            _context = context;
        }

        public string GenerateAccessToken(string username)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, username)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public async Task SaveRefreshTokenAsync(string username, string token)
        {
            // Remove old token
            var oldToken = _context.RefreshTokens.FirstOrDefault(x => x.Username == username);
            if (oldToken != null) _context.RefreshTokens.Remove(oldToken);

            var refreshToken = new RefreshToken
            {
                Username = username,
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ValidateRefreshTokenAsync(string username, string token)
        {
            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Username == username && x.Token == token);

            return stored != null && stored.ExpiryDate > DateTime.UtcNow;
        }
    }
}
