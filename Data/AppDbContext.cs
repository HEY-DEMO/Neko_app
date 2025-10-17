using Microsoft.EntityFrameworkCore;
using Neko_api.Models;
using System.Data;

namespace Neko_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<User_roles> User_roles { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User primary key
            modelBuilder.Entity<User>()
                .HasKey(u => u.Userid);

            modelBuilder.Entity<User_roles>()
                .HasKey(ur => new { ur.userid, ur.roleid });

            modelBuilder.Entity<User_roles>()
                .HasOne(ur => ur.User)       // use the navigation property
                .WithMany(u => u.UserRoles)  // collection in User
                .HasForeignKey(ur => ur.userid)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User_roles>()
                .HasOne(ur => ur.Role)       // use the navigation property
                .WithMany(r => r.UserRoles)  // collection in Roles
                .HasForeignKey(ur => ur.roleid)
                .OnDelete(DeleteBehavior.Cascade);


            // Optional: unique constraints for User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Mobile)
                .IsUnique();
        }
    }
}
