using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Sales
{
    public class InvoiceModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public InvoiceModel(PharmaSysDbContext db) => _db = db;

        public Sale Sale { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var sale = await _db.Sales
                .Include(s => s.Client)
                .Include(s => s.Prescription)
                .Include(s => s.Items).ThenInclude(i => i.Medication)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return RedirectToPage("Index");
            Sale = sale;
            return Page();
        }
    }
}
