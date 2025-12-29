using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;

namespace PharmaSys.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;

        public IndexModel(PharmaSysDbContext db)
        {
            _db = db;
        }

        // Cards
        public decimal TodayRevenue { get; set; }
        public int TodaySoldItems { get; set; }
        public int TodayClientsServed { get; set; }

        // Lists
        public List<TopProductVm> TopProducts { get; set; } = new();
        public List<ActivityVm> RecentActivity { get; set; } = new();

        public async Task OnGetAsync(string? period)
        {
            period ??= "week"; // default

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // ✅ Revenue today
            TodayRevenue = await _db.Sales
                .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow)
                .SumAsync(s => (decimal?)s.Total) ?? 0;

            // ✅ Sold items today (sum of quantities)
            TodaySoldItems = await _db.SaleItems
                .Where(i => i.Sale != null && i.Sale.SaleDate >= today && i.Sale.SaleDate < tomorrow)
                .SumAsync(i => (int?)i.Quantity) ?? 0;

            // ✅ Clients served today (distinct clients)
            TodayClientsServed = await _db.Sales
                .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow && s.ClientId != null)
                .Select(s => s.ClientId!.Value)
                .Distinct()
                .CountAsync();

            // ✅ Top Products (period-based)
            var (from, to) = GetRange(period);

            TopProducts = await _db.SaleItems
                .Where(i => i.Sale != null && i.Sale.SaleDate >= from && i.Sale.SaleDate < to)
                .GroupBy(i => new { i.MedicationId, Name = i.Medication != null ? i.Medication.Name : "Produit" })
                .Select(g => new TopProductVm
                {
                    MedicationId = g.Key.MedicationId,
                    Name = g.Key.Name,
                    Qty = g.Sum(x => x.Quantity),
                    Amount = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.Amount)
                .Take(4)
                .ToListAsync();

            // ✅ Recent Activity (sales + clients + supplier orders)
            var lastSales = await _db.Sales
                .OrderByDescending(s => s.SaleDate)
                .Take(4)
                .Select(s => new ActivityVm
                {
                    Title = $"Nouvelle vente - {s.InvoiceNo}",
                    Sub = $"Total : {s.Total:N2} DH",
                    When = s.SaleDate
                })
                .ToListAsync();

            var lastClients = await _db.Clients
                .OrderByDescending(c => c.Id)
                .Take(2)
                .Select(c => new ActivityVm
                {
                    Title = "Nouveau client enregistré",
                    Sub = c.Name ?? c.Code ?? "Client",
                    When = DateTime.Now // si tu n’as pas CreatedAt
                })
                .ToListAsync();

            var lastOrders = await _db.SupplierOrders
                .OrderByDescending(o => o.Id)
                .Take(2)
                .Select(o => new ActivityVm
                {
                    Title = "Réapprovisionnement effectué",
                    Sub = $"Commande : {o.OrderNumber}",
                    When = o.OrderDate
                })
                .ToListAsync();

            RecentActivity = lastSales
                .Concat(lastClients)
                .Concat(lastOrders)
                .OrderByDescending(x => x.When)
                .Take(6)
                .ToList();
        }

        // ====== JSON HANDLERS (Charts) ======

        public async Task<IActionResult> OnGetSalesChartAsync(string period = "week")
        {
            var (from, to) = GetRange(period);

            // Group by day
            var data = await _db.Sales
                .Where(s => s.SaleDate >= from && s.SaleDate < to)
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new { date = g.Key, total = g.Sum(x => x.Total) })
                .OrderBy(x => x.date)
                .ToListAsync();

            var labels = data.Select(x => x.date.ToString("ddd dd")).ToList();
            var values = data.Select(x => x.total).ToList();

            return new JsonResult(new { labels, values });
        }

        public async Task<IActionResult> OnGetInventoryChartAsync()
        {
            // ⚠️ Adapte ces champs selon ton modèle Medication
            // Exemple supposé:
            // Medication.StockQty, Medication.MinStock, Medication.ExpiryDate

            var meds = await _db.Medications.ToListAsync();

            int total = meds.Count;

            int outOfStock = meds.Count(m => m.StockQty <= 0);
            int lowStock = meds.Count(m => m.StockQty > 0 && m.StockQty <= m.MinStock);


            var soon = DateTime.Today.AddDays(30);

            int expireSoon = meds.Count(m =>
                m.ExpiryDate != null &&
                ((DateTime)m.ExpiryDate).Date <= soon
            );

            return new JsonResult(new
            {
                labels = new[] { "Total produit", "Rupture", "Stock faible", "Expire bientôt" },
                values = new[] { total, outOfStock, lowStock, expireSoon }
            });
        }

        private static (DateTime from, DateTime to) GetRange(string period)
        {
            var today = DateTime.Today;

            if (period == "month")
            {
                var from = new DateTime(today.Year, today.Month, 1);
                return (from, from.AddMonths(1));
            }

            // week default
            // week starts Monday
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var monday = today.AddDays(-diff);
            return (monday, monday.AddDays(7));
        }
    }

    public class TopProductVm
    {
        public int MedicationId { get; set; }
        public string Name { get; set; } = "";
        public int Qty { get; set; }
        public decimal Amount { get; set; }
    }

    public class ActivityVm
    {
        public string Title { get; set; } = "";
        public string Sub { get; set; } = "";
        public DateTime When { get; set; }
    }
}