using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.SupplierOrders
{
    public class DetailsModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public DetailsModel(PharmaSysDbContext db) => _db = db;

        public SupplierOrder Order { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var order = await _db.SupplierOrders
                .Include(o => o.Supplier)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Medication)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return RedirectToPage("Index");

            Order = order;
            return Page();
        }

        // Bouton "Réceptionner"
        public async Task<IActionResult> OnPostReceiveAsync(int id)
        {
            var order = await _db.SupplierOrders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return RedirectToPage("Index");

            // Si déjà reçu => rien à faire
            if (order.Status == "Received")
                return RedirectToPage(new { id });

            // Mettre à jour le stock pour chaque item
            foreach (var it in order.Items)
            {
                var med = await _db.Medications.FirstOrDefaultAsync(m => m.Id == it.MedicationId);
                if (med != null)
                {
                    med.Stock += it.Quantity;
                    // Optionnel: mettre à jour PurchasePrice si tu veux garder le dernier coût
                    // med.PurchasePrice = it.UnitCost;
                    _db.Medications.Update(med);
                }
            }

            order.Status = "Received";
            _db.SupplierOrders.Update(order);

            await _db.SaveChangesAsync();

            return RedirectToPage(new { id });
        }

        // Bouton "Annuler commande"
        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var order = await _db.SupplierOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return RedirectToPage("Index");

            if (order.Status != "Received")
            {
                order.Status = "Cancelled";
                _db.SupplierOrders.Update(order);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage(new { id });
        }
    }
}
