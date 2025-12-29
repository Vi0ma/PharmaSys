using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Prescriptions
{
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public IndexModel(PharmaSysDbContext db) => _db = db;

        public List<Prescription> Prescriptions { get; set; } = new();

        public async Task OnGetAsync()
        {
            Prescriptions = await _db.Prescriptions
                .Include(p => p.Client)
                .OrderByDescending(p => p.IssueDate)
                .ToListAsync();
        }
    }
}
