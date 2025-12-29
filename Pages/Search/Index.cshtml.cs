using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Search
{
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public IndexModel(PharmaSysDbContext db) => _db = db;

        public string Query { get; set; } = "";
        public List<Medication> Medications { get; set; } = new();
        public List<Client> Clients { get; set; } = new();
        public List<Sale> Sales { get; set; } = new();

        public async Task OnGetAsync(string? q)
        {
            Query = (q ?? "").Trim();
            if (string.IsNullOrWhiteSpace(Query)) return;

            var term = Query.ToLower();

            Medications = await _db.Medications
                .Where(m => (m.Name + " " + m.Code).ToLower().Contains(term))
                .OrderBy(m => m.Name)
                .Take(15)
                .ToListAsync();

            Clients = await _db.Clients
                .Where(c => (c.FullName + " " + c.Code).ToLower().Contains(term))
                .OrderBy(c => c.FullName)
                .Take(15)
                .ToListAsync();

            Sales = await _db.Sales
                .Where(s => s.InvoiceNo.ToLower().Contains(term))
                .OrderByDescending(s => s.SaleDate)
                .Take(15)
                .ToListAsync();
        }
    }
}
