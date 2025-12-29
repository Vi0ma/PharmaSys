using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaSys.Models
{
    public class Sale
    {
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string InvoiceNo { get; set; } = "";

        public DateTime SaleDate { get; set; } = DateTime.Now; // ✅ IMPORTANT

        public int? ClientId { get; set; }
        public Client? Client { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Paid { get; set; }

        [Required, StringLength(20)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash/Card/Credit

        [Required, StringLength(20)]
        public string Status { get; set; } = "Paid"; // Paid/Partial/Unpaid

        public int? PrescriptionId { get; set; }
        public Prescription? Prescription { get; set; }

        public List<SaleItem> Items { get; set; } = new();
    }
}