using Microsoft.EntityFrameworkCore;
using RTMAuthServer.Models;

namespace RTMAuthServer.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}