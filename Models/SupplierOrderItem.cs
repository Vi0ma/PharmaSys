using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaSys.Models
{
    public class SupplierOrderItem
    {
        public int Id { get; set; }

        [Required]
        public int SupplierOrderId { get; set; }
        public SupplierOrder? SupplierOrder { get; set; }

        [Required]
        public int MedicationId { get; set; }
        public Medication? Medication { get; set; }

        [Range(1, 999999)]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }          // ✅ Colonne à ajouter en DB
    }
}
