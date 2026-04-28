using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShoppingZone.Wallet.API.Entities
{
    public class EWallet
    {
        [Key]
        public int WalletId { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        public IList<Statement> Statements { get; set; } = new List<Statement>();
    }
}