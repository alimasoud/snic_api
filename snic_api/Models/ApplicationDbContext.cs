using Microsoft.EntityFrameworkCore;
using snic_api.Models;

namespace snic_api.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<BlacklistedToken> BlacklistedTokens { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<Policy> Policies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configure BlacklistedToken
            modelBuilder.Entity<BlacklistedToken>()
                .HasIndex(bt => bt.TokenId)
                .IsUnique();

            modelBuilder.Entity<BlacklistedToken>()
                .HasIndex(bt => bt.ExpiresAt);

            // Configure relationship
            modelBuilder.Entity<BlacklistedToken>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(bt => bt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Product relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete of user

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Features)
                .WithOne(f => f.Product)
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // Delete features when product is deleted

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Policies)
                .WithOne(pol => pol.Product)
                .HasForeignKey(pol => pol.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete of policies

            // Configure Feature relationships
            modelBuilder.Entity<Feature>()
                .HasOne(f => f.Product)
                .WithMany(p => p.Features)
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Policy relationships
            modelBuilder.Entity<Policy>()
                .HasOne(pol => pol.Product)
                .WithMany(p => p.Policies)
                .HasForeignKey(pol => pol.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Policy>()
                .HasOne(pol => pol.User)
                .WithMany()
                .HasForeignKey(pol => pol.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure indexes
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name);

            modelBuilder.Entity<Policy>()
                .HasIndex(pol => pol.PolicyNumber)
                .IsUnique();

            // Configure decimal precision
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Policy>()
                .Property(pol => pol.Premium)
                .HasPrecision(18, 2);
        }
    }
} 