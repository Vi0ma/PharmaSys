using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public IndexModel(PharmaSysDbContext db) => _db = db;

        public List<Client> Clients { get; set; } = new();
        public Dictionary<int, decimal> TotalPurchases { get; set; } = new();

        public async Task OnGetAsync()
        {
            Clients = await _db.Clients
                .OrderByDescending(c => c.LastVisit ?? DateTime.MinValue)
                .ToListAsync();

            // Total achats = somme des Sales.Total par client
            TotalPurchases = await _db.Sales
                .Where(s => s.ClientId != null)
                .GroupBy(s => s.ClientId!.Value)
                .Select(g => new { ClientId = g.Key, Total = g.Sum(x => x.Total) })
                .ToDictionaryAsync(x => x.ClientId, x => x.Total);
        }
    }
}
