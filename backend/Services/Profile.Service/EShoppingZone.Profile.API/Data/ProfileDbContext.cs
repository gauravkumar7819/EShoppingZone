using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EShoppingZone.Profile.API.Entities;

namespace EShoppingZone.Profile.API.Data
{
    public class ProfileDbContext : DbContext
    {
        public ProfileDbContext(DbContextOptions<ProfileDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserProfile>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<UserProfile>()
                .HasIndex(u => u.MobileNumber)
                .IsUnique();

            modelBuilder.Entity<UserProfile>()
                .HasIndex(u => u.GitHubId)
                .IsUnique();

            modelBuilder.Entity<UserProfile>()
                .HasMany(u => u.Addresses)
                .WithOne(a => a.UserProfile)
                .HasForeignKey(a => a.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Generate hash properly
            var hasher = new PasswordHasher<UserProfile>();

            var adminUser = new UserProfile
            {
                Id = 1,
                FullName = "Admin User",
                Email = "admin@eshoppingzone.com",
                MobileNumber = "9999999999",
                Role = "Admin",
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

            modelBuilder.Entity<UserProfile>().HasData(adminUser);
        }
    }
}