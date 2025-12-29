using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Models
{
    public class Prescription
    {
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string Number { get; set; } = "";

        [Required, StringLength(120)]
        public string DoctorName { get; set; } = "";

        public DateTime IssueDate { get; set; } = DateTime.Today;

        public DateTime ExpiryDate { get; set; } = DateTime.Today.AddDays(30);

        [StringLength(250)]
        public string? Notes { get; set; }

        // Optionnel: lier au client
        public int? ClientId { get; set; }
        public Client? Client { get; set; }

        public List<Sale> Sales { get; set; } = new();
    }
}
