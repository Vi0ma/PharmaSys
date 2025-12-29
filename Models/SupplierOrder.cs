using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Models
{
    public class SupplierOrder
    {
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string OrderNumber { get; set; } = "";

        [Required]
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [Required, StringLength(20)]
        public string Status { get; set; } = "Draft";

        public string? Notes { get; set; }

        public List<SupplierOrderItem> Items { get; set; } = new();
    }
}