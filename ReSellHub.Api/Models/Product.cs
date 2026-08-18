using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReSellHub.Api.Models
{
    public class Product
    {
        public int Id { get; set; }

        public int? CategoryId { get; set; }
        public int? SellerId { get; set; }

        [Required(ErrorMessage = "商品名稱為必填欄位。")]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "成本不得小於 0。")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "售價不得小於 0。")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "庫存數量不得小於 0。")]
        public int StockQuantity { get; set; }

        [StringLength(2000)] public string Description { get; set; } = string.Empty;
        [StringLength(30)] public string Condition { get; set; } = "二手良品";
        [StringLength(500)] public string? CoverImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Category? Category { get; set; }
        public AppUser? Seller { get; set; }
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
