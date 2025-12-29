using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaSys.Data;
using PharmaSys.Models;
using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Pages.Suppliers
{
    [Authorize(Roles = "Admin,Pharmacien")]
    public class CreateModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public CreateModel(PharmaSysDbContext db) => _db = db;

        [BindProperty] public SupplierInput Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var supplier = new Supplier
            {
                Name = Input.Name.Trim(),
                Phone = Input.Phone?.Trim(),
                Email = Input.Email?.Trim(),
                Address = Input.Address?.Trim(),
                Notes = Input.Notes?.Trim()
            };

            _db.Suppliers.Add(supplier);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Fournisseur ajouté avec succès.";
            return RedirectToPage("Index");
        }

        public class SupplierInput
        {
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
