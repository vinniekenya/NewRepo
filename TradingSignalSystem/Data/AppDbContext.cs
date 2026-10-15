using Microsoft.EntityFrameworkCore;
using TradingSignalsApp.Models;

namespace TradingSignalsApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // TABLES
        public DbSet<User> Users { get; set; }
        public DbSet<Signal> Signals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //  Seed default admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "Admin",
                    Email = "admin@trading.com",
                    Password = "admin123",
                    Role = "Admin",
                    IsApproved = true,
                    HasPaid = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );
        }
    }
}