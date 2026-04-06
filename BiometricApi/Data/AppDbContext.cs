using BiometricApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace BiometricApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        //public DbSet<Biometric> Biometrics { get; set; }

        public DbSet<Demographic> Demographics { get; set; }
        public DbSet<Organization> Organizations { get; set; } 
    }
}
