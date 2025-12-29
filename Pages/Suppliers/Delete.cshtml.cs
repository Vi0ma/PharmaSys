using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;

namespace PharmaSys.Pages.Suppliers
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public DeleteModel(PharmaSysDbContext db) => _db = db;

        [BindProperty] public int Id { get; set; }

        public string SupplierName { get; set; } = "";
        public string SupplierPhone { get; set; } = "—";
        public string SupplierEmail { get; set; } = "—";
        public string SupplierAddress { get; set; } = "—";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var s = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (s == null) return NotFound();

            Id = s.Id;
            SupplierName = s.Name;
            SupplierPhone = string.IsNullOrWhiteSpace(s.Phone) ? "—" : s.Phone!;
            SupplierEmail = string.IsNullOrWhiteSpace(s.Email) ? "—" : s.Email!;
            SupplierAddress = string.IsNullOrWhiteSpace(s.Address) ? "—" : s.Address!;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var s = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == Id);
            if (s == null) return NotFound();

            _db.Suppliers.Remove(s);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Fournisseur supprimé.";
            return RedirectToPage("Index");
        }
    }
}
