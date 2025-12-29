using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = "";

        [StringLength(30)]
        public string? Phone { get; set; }

        [StringLength(120)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        // ✅ AJOUTE ÇA
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
