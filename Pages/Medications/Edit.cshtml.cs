using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;
using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Pages.Medications
{
    [Authorize(Roles = "Admin,Pharmacien")]
    public class EditModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public EditModel(PharmaSysDbContext db) => _db = db;

        public List<Category> Categories { get; set; } = new();
        public List<Supplier> Suppliers { get; set; } = new();

        [BindProperty] public InputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0) return RedirectToPage("/Medications/Index");

            Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync();
            Suppliers = await _db.Suppliers.OrderBy(x => x.Name).ToListAsync();

            var med = await _db.Medications.FirstOrDefaultAsync(x => x.Id == id);
            if (med == null) return RedirectToPage("/Medications/Index");

            Input = new InputModel
            {
                Id = med.Id,
                Code = med.Code,
                Name = med.Name,
                Dosage = med.Dosage,
                Manufacturer = med.Manufacturer,
                Barcode = med.Barcode,
                CategoryId = med.CategoryId,
                SupplierId = med.SupplierId,
                Stock = med.Stock,
                StockMin = med.StockMin,
                PurchasePrice = med.PurchasePrice,
                SalePrice = med.SalePrice,
                ExpiryDate = med.ExpirationDate,
                Location = med.Location
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // IMPORTANT: recharger listes si on revient sur la page (validation error)
            Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync();
            Suppliers = await _db.Suppliers.OrderBy(x => x.Name).ToListAsync();

            if (!ModelState.IsValid) return Page();

            var med = await _db.Medications.FirstOrDefaultAsync(x => x.Id == Input.Id);
            if (med == null) return RedirectToPage("/Medications/Index");

            med.Code = Input.Code.Trim();
            med.Name = Input.Name.Trim();
            med.Dosage = Input.Dosage?.Trim();
            med.Manufacturer = Input.Manufacturer?.Trim();
            med.Barcode = Input.Barcode?.Trim();
            med.CategoryId = Input.CategoryId;
            med.SupplierId = Input.SupplierId;

            med.Stock = Input.Stock;
            med.StockMin = Input.StockMin;
            med.PurchasePrice = Input.PurchasePrice;
            med.SalePrice = Input.SalePrice;
            med.ExpirationDate = Input.ExpiryDate;
            med.Location = Input.Location?.Trim();

            await _db.SaveChangesAsync();

            TempData["Success"] = "Médicament mis à jour avec succès.";
            return RedirectToPage("/Medications/Index");
        }

        public class InputModel
        {
            public int Id { get; set; }

            [Required] public string Code { get; set; } = "";
            [Required] public string Name { get; set; } = "";

            public string? Dosage { get; set; }
            public string? Manufacturer { get; set; }
            public string? Barcode { get; set; }

            public int? CategoryId { get; set; }
            public int? SupplierId { get; set; }

            public int Stock { get; set; } = 0;
            public int StockMin { get; set; } = 0;

            public decimal PurchasePrice { get; set; } = 0;
            public decimal SalePrice { get; set; } = 0;

            public DateTime? ExpiryDate { get; set; }
            public string? Location { get; set; }
        }
    }
}
