using Microsoft.EntityFrameworkCore;
using TP2_API.Models;

namespace TP2_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Use the "admin" schema for the Users table
            modelBuilder.HasDefaultSchema("admin");

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("users", "admin"); 
                entity.HasKey(u => u.UserId);
                entity.Property(e => e.LastLoginDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
        }

    }
}