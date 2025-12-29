using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Medications
{
    public class CreateModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public CreateModel(PharmaSysDbContext db) => _db = db;

        [BindProperty] public Medication Medication { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Supplier> Suppliers { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync();
            Suppliers = await _db.Suppliers.OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            _db.Medications.Add(Medication);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
