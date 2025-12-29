using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Prescriptions
{
    public class CreateModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public CreateModel(PharmaSysDbContext db) => _db = db;

        [BindProperty] public Prescription Prescription { get; set; } = new();
        public List<Client> Clients { get; set; } = new();

        public async Task OnGetAsync()
        {
            Clients = await _db.Clients.OrderBy(c => c.FullName).ToListAsync();
            Prescription.IssueDate = DateTime.Today;
            Prescription.ExpiryDate = DateTime.Today.AddDays(30);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Clients = await _db.Clients.OrderBy(c => c.FullName).ToListAsync();
            if (!ModelState.IsValid) return Page();

            _db.Prescriptions.Add(Prescription);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
