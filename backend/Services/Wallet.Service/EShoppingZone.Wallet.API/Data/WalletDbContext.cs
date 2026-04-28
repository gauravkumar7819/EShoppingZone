using Microsoft.EntityFrameworkCore;
using EShoppingZone.Wallet.API.Entities;

namespace EShoppingZone.Wallet.API.Data
{
    public class WalletDbContext : DbContext
    {
        public WalletDbContext(DbContextOptions<WalletDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<EWallet> Wallets { get; set; }
        public DbSet<Statement> Statements { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships
            modelBuilder.Entity<EWallet>()
                .HasMany(w => w.Statements)
                .WithOne(s => s.Wallet)
                .HasForeignKey(s => s.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure unique constraint on CustomerId
            modelBuilder.Entity<EWallet>()
                .HasIndex(w => w.CustomerId)
                .IsUnique()
                .HasDatabaseName("IX_Wallet_CustomerId");
            
            // Configure indexes for better performance
            modelBuilder.Entity<Statement>()
                .HasIndex(s => s.TransactionDate)
                .HasDatabaseName("IX_Statement_TransactionDate");
            
            modelBuilder.Entity<Statement>()
                .HasIndex(s => s.WalletId)
                .HasDatabaseName("IX_Statement_WalletId");
            
            modelBuilder.Entity<Statement>()
                .HasIndex(s => s.OrderId)
                .HasDatabaseName("IX_Statement_OrderId");
            
            modelBuilder.Entity<Statement>()
                .HasIndex(s => s.RazorpayPaymentId)
                .HasDatabaseName("IX_Statement_RazorpayPaymentId");
            
            // Configure decimal precision
            modelBuilder.Entity<EWallet>()
                .Property(w => w.CurrentBalance)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Statement>()
                .Property(s => s.Amount)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Statement>()
                .Property(s => s.BalanceAfterTransaction)
                .HasPrecision(18, 2);
        }
    }
}