using Microsoft.EntityFrameworkCore;
using EShoppingZone.Product.API.Entities;
using ProductModel = EShoppingZone.Product.API.Entities.Product;

namespace EShoppingZone.Product.API.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<ProductModel> Products { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure indexes for better performance
            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => p.Name)
                .HasDatabaseName("IX_Product_Name");
                
            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => p.Category)
                .HasDatabaseName("IX_Product_Category");
                
            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => p.Type)
                .HasDatabaseName("IX_Product_Type");
                
            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => p.MerchantId)
                .HasDatabaseName("IX_Product_MerchantId");
                
            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => new { p.Category, p.IsActive })
                .HasDatabaseName("IX_Product_Category_Active");
            
            // Seed sample data
            modelBuilder.Entity<ProductModel>().HasData(
                new ProductModel
                {
                    Id = 1,
                    Name = "iPhone 15 Pro",
                    Description = "Latest Apple iPhone with A17 Pro chip",
                    Category = "Electronics",
                    Type = "Mobile",
                    Price = 120000,
                    MRP = 150000,
                    StockQuantity = 50,
                    Brand = "Apple",
                    MerchantId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ProductModel
                {
                    Id = 2,
                    Name = "The Great Gatsby",
                    Description = "Classic novel by F. Scott Fitzgerald",
                    Category = "Books",
                    Type = "Fiction",
                    Price = 299,
                    MRP = 499,
                    StockQuantity = 100,
                    Brand = "Penguin",
                    MerchantId = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ProductModel
                {
                    Id = 3,
                    Name = "Nike Air Max",
                    Description = "Comfortable running shoes",
                    Category = "Apparel",
                    Type = "Shoes",
                    Price = 8999,
                    MRP = 12999,
                    StockQuantity = 75,
                    Brand = "Nike",
                    MerchantId = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ProductModel
                {
                    Id = 4,
                    Name = "Dove Shampoo",
                    Description = "Nourishing hair care",
                    Category = "Personal Care",
                    Type = "Hair Care",
                    Price = 350,
                    MRP = 450,
                    StockQuantity = 200,
                    Brand = "Dove",
                    MerchantId = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}