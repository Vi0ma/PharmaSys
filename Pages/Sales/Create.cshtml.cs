using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;
using System.ComponentModel.DataAnnotations;

namespace PharmaSys.Pages.Sales
{
    public class CreateModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public CreateModel(PharmaSysDbContext db) => _db = db;

        public List<Medication> Medications { get; set; } = new();
        public List<Client> Clients { get; set; } = new();
        public List<Prescription> Prescriptions { get; set; } = new();

        [BindProperty] public InputModel Input { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadListsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadListsAsync();

            if (!ModelState.IsValid)
                return Page();

            if (Input.Items == null || Input.Items.Count == 0)
            {
                ModelState.AddModelError("", "Ajoute au moins un médicament au panier.");
                return Page();
            }

            // ✅ Générer InvoiceNo unique
            var now = DateTime.Now;
            var prefix = $"INV-{now:yyyy}-";
            var lastInvoice = await _db.Sales
                .Where(s => s.InvoiceNo.StartsWith(prefix))
                .OrderByDescending(s => s.InvoiceNo)
                .Select(s => s.InvoiceNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrWhiteSpace(lastInvoice))
            {
                // ex: INV-2025-023
                var parts = lastInvoice.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int n))
                    nextNumber = n + 1;
            }

            var invoiceNo = $"{prefix}{nextNumber:D3}";

            // ✅ Calcul total
            decimal total = 0;
            foreach (var it in Input.Items)
            {
                if (it.Quantity < 1) it.Quantity = 1;
                if (it.UnitPrice < 0) it.UnitPrice = 0;
                if (it.Discount < 0) it.Discount = 0;

                total += (it.UnitPrice * it.Quantity) - it.Discount;
            }
            if (total < 0) total = 0;

            var paid = Input.Paid < 0 ? 0 : Input.Paid;

            string status;
            if (paid <= 0) status = "Unpaid";
            else if (paid >= total) status = "Paid";
            else status = "Partial";

            int? clientId = Input.ClientId == 0 ? null : Input.ClientId;

            int? prescriptionId = string.IsNullOrWhiteSpace(Input.PrescriptionId)
                ? null
                : (int?)int.Parse(Input.PrescriptionId);

            var sale = new Sale
            {
                InvoiceNo = invoiceNo,
                SaleDate = now, // ✅ IMPORTANT
                ClientId = clientId,
                PrescriptionId = prescriptionId,
                PaymentMethod = Input.PaymentMethod ?? "Cash",
                Total = total,
                Paid = paid,
                Status = status
            };

            _db.Sales.Add(sale);
            await _db.SaveChangesAsync(); // ✅ Sale.Id

            // ✅ Items + décrément stock
            foreach (var it in Input.Items)
            {
                var med = await _db.Medications.FirstOrDefaultAsync(m => m.Id == it.MedicationId);
                if (med == null) continue;

                if (it.Quantity > med.Stock)
                {
                    ModelState.AddModelError("", $"Stock insuffisant pour {med.Name}. Stock actuel: {med.Stock}");
                    return Page();
                }

                med.Stock -= it.Quantity;

                _db.SaleItems.Add(new SaleItem
                {
                    SaleId = sale.Id,
                    MedicationId = it.MedicationId,
                    Quantity = it.Quantity,
                    UnitPrice = it.UnitPrice,
                    Discount = it.Discount
                });
            }

            await _db.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        private async Task LoadListsAsync()
        {
            Medications = await _db.Medications
                .OrderBy(m => m.Name)
                .ToListAsync();

            Clients = await _db.Clients
                .OrderBy(c => c.FullName)
                .ToListAsync();

            // seulement ordonnances valides (optionnel)
            var today = DateTime.Today;
            Prescriptions = await _db.Prescriptions
                .Where(p => p.ExpiryDate >= today)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }

        public class InputModel
        {
            public int ClientId { get; set; } = 0; // 0 = vente rapide
            public string? PrescriptionId { get; set; } // on reçoit string depuis hidden
            public string? PaymentMethod { get; set; } = "Cash";
            public decimal Paid { get; set; } = 0;

            public List<ItemInput> Items { get; set; } = new();
        }

        public class ItemInput
        {
            public int MedicationId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Discount { get; set; }
        }
    }
}