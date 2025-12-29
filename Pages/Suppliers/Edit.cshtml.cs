using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Pages.Suppliers
{
    [Authorize(Roles = "Admin,Pharmacien")]
    public class EditModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public EditModel(PharmaSysDbContext db) => _db = db;

        [BindProperty] public SupplierInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var s = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
            if (s == null) return NotFound();

            Input = new SupplierInput
            {
                Id = s.Id,
                Name = s.Name,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                Notes = s.Notes
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var s = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == Input.Id);
            if (s == null) return NotFound();

            s.Name = Input.Name.Trim();
            s.Phone = Input.Phone?.Trim();
            s.Email = Input.Email?.Trim();
            s.Address = Input.Address?.Trim();
            s.Notes = Input.Notes?.Trim();

            await _db.SaveChangesAsync();
            TempData["Success"] = "Fournisseur modifié avec succès.";
            return RedirectToPage("Index");
        }

        public class SupplierInput
        {
            public int Id { get; set; }

            [Required, StringLength(120)]
            public string Name { get; set; } = "";

            [StringLength(40)]
            public string? Phone { get; set; }

            [EmailAddress, StringLength(120)]
            public string? Email { get; set; }

            [StringLength(200)]
            public string? Address { get; set; }

            [StringLength(500)]
            public string? Notes { get; set; }
        }
    }
}
