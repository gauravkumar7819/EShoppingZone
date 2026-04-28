using Microsoft.EntityFrameworkCore;
using EShoppingZone.Cart.API.Entities;
using CartModel = EShoppingZone.Cart.API.Entities.Cart;

namespace EShoppingZone.Cart.API.Data
{
    public class CartDbContext : DbContext
    {
        public CartDbContext(DbContextOptions<CartDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<CartModel> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships
            modelBuilder.Entity<CartModel>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure indexes
            modelBuilder.Entity<CartModel>()
                .HasIndex(c => c.UserId)
                .IsUnique()
                .HasDatabaseName("IX_Cart_UserId");
            
            modelBuilder.Entity<CartItem>()
                .HasIndex(i => new { i.CartId, i.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_CartItem_CartId_ProductId");
            
            // Configure decimal precision
            modelBuilder.Entity<CartItem>()
                .Property(i => i.Price)
                .HasPrecision(18, 2);
        }
    }
}