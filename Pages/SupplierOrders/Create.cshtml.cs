using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;
using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Pages.SupplierOrders
{
    public class CreateModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public CreateModel(PharmaSysDbContext db) => _db = db;

        public List<Medication> Medications { get; set; } = new();
        public SelectList SupplierOptions { get; set; } = null!;

        [BindProperty] public InputModel Input { get; set; } = new();

        public async Task OnGetAsync()
        {
            Medications = await _db.Medications.OrderBy(x => x.Name).ToListAsync();
            SupplierOptions = new SelectList(await _db.Suppliers.OrderBy(s => s.Name).ToListAsync(), "Id", "Name");

            Input.OrderDate = DateTime.Today;
            Input.Status = "Draft";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Medications = await _db.Medications.OrderBy(x => x.Name).ToListAsync();
            SupplierOptions = new SelectList(await _db.Suppliers.OrderBy(s => s.Name).ToListAsync(), "Id", "Name");

            if (!ModelState.IsValid) return Page();

            if (Input.Items == null || Input.Items.Count == 0)
            {
                ModelState.AddModelError("", "Ajoute au moins un article.");
                return Page();
            }

            // ✅ Générer OrderNumber unique: SO-2025-001
            var year = DateTime.Now.Year;
            var prefix = $"SO-{year}-";

            var last = await _db.SupplierOrders
                .Where(x => x.OrderNumber.StartsWith(prefix))
                .OrderByDescending(x => x.OrderNumber)
                .Select(x => x.OrderNumber)
                .FirstOrDefaultAsync();

            int next = 1;
            if (!string.IsNullOrWhiteSpace(last))
            {
                // last: "SO-2025-001"
                var parts = last.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3 && int.TryParse(parts[2], out var n))
                    next = n + 1;
            }

            var orderNumber = $"{prefix}{next:000}";

            var order = new SupplierOrder
            {
                SupplierId = Input.SupplierId,
                OrderDate = Input.OrderDate,
                Status = Input.Status ?? "Draft",
                Notes = Input.Notes,
                OrderNumber = orderNumber // ✅ IMPORTANT
            };

            _db.SupplierOrders.Add(order);
            await _db.SaveChangesAsync();

            foreach (var it in Input.Items)
            {
                _db.SupplierOrderItems.Add(new SupplierOrderItem
                {
                    SupplierOrderId = order.Id,
                    MedicationId = it.MedicationId,
                    Quantity = it.Quantity,
                    UnitPrice = it.UnitPrice
                });
            }

            await _db.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        public class InputModel
        {
            [Required]
            public int SupplierId { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime OrderDate { get; set; } = DateTime.Today;

            public string? Status { get; set; } = "Draft";
            public string? Notes { get; set; }

            public List<ItemInput> Items { get; set; } = new();
        }

        public class ItemInput
        {
            public int MedicationId { get; set; }
            public int Quantity { get; set; }

            [Range(0, 999999)]
            public decimal UnitPrice { get; set; }
        }
    }
}