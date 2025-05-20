using Microsoft.EntityFrameworkCore;
using TeploobmenStenkaIIIWeb.Models.Entities;

namespace TeploobmenStenkaIIIWeb.Services
{
    public class AppDbContext(IWebHostEnvironment env) : DbContext
    {
        public DbSet<BioCoeff> BioCoeffs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var pathToDb = Path.Combine(env.ContentRootPath, "appDataBase.db");
            optionsBuilder.UseSqlite($"Data Source={pathToDb}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BioCoeff>()
                .HasIndex(x => new { x.Bio })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
