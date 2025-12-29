using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaSys.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Code { get; set; } = "CLI001";

        [Required, StringLength(150)]
        public string FullName { get; set; } = "";

        [StringLength(30)]
        public string? Phone { get; set; }

        [StringLength(120)]
        public string? Email { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        public DateTime? LastVisit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; } = 0;
        public string? Name { get; internal set; }
    }
}
