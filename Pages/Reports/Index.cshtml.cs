using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;

namespace PharmaSys.Pages.Reports
{
    [Authorize(Roles = "Admin,Pharmacien")]

    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public IndexModel(PharmaSysDbContext db) => _db = db;

        public decimal TodaySales { get; set; }
        public decimal MonthSales { get; set; }
        public decimal YearSales { get; set; }

        public List<TopProductRow> TopProducts { get; set; } = new();
        public decimal ProfitEstimate { get; set; }

        public async Task OnGetAsync()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var yearStart = new DateTime(today.Year, 1, 1);

            TodaySales = await _db.Sales.Where(s => s.SaleDate.Date == today).SumAsync(s => (decimal?)s.Total) ?? 0;
            MonthSales = await _db.Sales.Where(s => s.SaleDate >= monthStart).SumAsync(s => (decimal?)s.Total) ?? 0;
            YearSales = await _db.Sales.Where(s => s.SaleDate >= yearStart).SumAsync(s => (decimal?)s.Total) ?? 0;

            TopProducts = await _db.SaleItems
                .Include(i => i.Medication)
                .GroupBy(i => new { i.MedicationId, Name = i.Medication!.Name, Dosage = i.Medication!.Dosage })
                .Select(g => new TopProductRow
                {
                    Name = g.Key.Name + " " + g.Key.Dosage,
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => (x.UnitPrice * x.Quantity) - x.Discount)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(10)
                .ToListAsync();

            // Profit approximatif (vente - achat) si PurchasePrice existe dans Medication
            ProfitEstimate = await _db.SaleItems
                .Include(i => i.Medication)
                .SumAsync(i => ((i.UnitPrice - (i.Medication!.PurchasePrice)) * i.Quantity) - i.Discount);
        }

        public class TopProductRow
        {
            public string Name { get; set; } = "";
            public int Quantity { get; set; }
            public decimal Revenue { get; set; }
        }
    }
}
