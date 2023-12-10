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
            // Use the "admin" schema for the Users, UserRole, and UserRoleMapping tables
            modelBuilder.HasDefaultSchema("admin");

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("users", "admin"); // Specify the schema name if it's not the default one
                entity.HasKey(u => u.UserId);
                entity.Property(e => e.LastLoginDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
        }

    }
}