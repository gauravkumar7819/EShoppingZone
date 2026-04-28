using Microsoft.EntityFrameworkCore;
using EShoppingZone.Order.API.Entities;
using OrderModel=EShoppingZone.Order.API.Entities.Order;
namespace EShoppingZone.Order.API.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<StatusHistory> StatusHistories { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships
            modelBuilder.Entity<OrderModel>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<OrderModel>()
                .HasMany(o => o.StatusHistories)
                .WithOne(h => h.Order)
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure indexes
            modelBuilder.Entity<OrderModel>()
                .HasIndex(o => o.CustomerId)
                .HasDatabaseName("IX_Order_CustomerId");
            
            modelBuilder.Entity<OrderModel>()
                .HasIndex(o => o.OrderDate)
                .HasDatabaseName("IX_Order_OrderDate");
            
            modelBuilder.Entity<OrderModel>()
                .HasIndex(o => o.Status)
                .HasDatabaseName("IX_Order_Status");
            
            modelBuilder.Entity<OrderModel>()
                .HasIndex(o => new { o.CustomerId, o.OrderDate })
                .HasDatabaseName("IX_Order_Customer_OrderDate");
            
            // Configure decimal precision
            modelBuilder.Entity<OrderModel>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<OrderItem>()
                .Property(i => i.Price)
                .HasPrecision(18, 2);
        }
    }
}