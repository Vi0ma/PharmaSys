using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Sales
{
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public IndexModel(PharmaSysDbContext db) => _db = db;

        public List<Sale> Sales { get; set; } = new();

        public async Task OnGetAsync()
        {
            Sales = await _db.Sales
                .Include(s => s.Client)
                .OrderByDescending(s => s.SaleDate) 
                .ToListAsync();
        }
    }
}