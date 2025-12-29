using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Sales
{
    public class DetailsModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public DetailsModel(PharmaSysDbContext db) => _db = db;

        public Sale Sale { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var sale = await _db.Sales
                .Include(s => s.Client)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Medication)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return NotFound();

            Sale = sale;
            return Page();
        }
    }
}