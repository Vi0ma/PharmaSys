using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaSys.Models
{
    public class Medication
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Code { get; set; } = "MED001";

        [Required, StringLength(150)]
        public string Name { get; set; } = "";

        [StringLength(60)]
        public string? Form { get; set; } // comprimé, sirop...

        [StringLength(60)]
        public string? Dosage { get; set; } // 500mg

        [StringLength(120)]
        public string? Manufacturer { get; set; }

        [StringLength(50)]
        public string? Barcode { get; set; }

        [Required]
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public int Stock { get; set; } = 0;
        public int StockMin { get; set; } = 0;

        [StringLength(30)]
        public string? Location { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; } = 0;

        public DateTime? ExpirationDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; internal set; }
        public int StockQty { get; internal set; }
        public int MinStock { get; internal set; }
    }
}
