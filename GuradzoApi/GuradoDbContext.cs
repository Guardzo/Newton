using GuradzoApi.Data;
using GuradzoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GuradzoApi
{
    public class GuradoDbContext : DbContext
    {
        public GuradoDbContext(DbContextOptions<GuradoDbContext> options) : base(options) { }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
