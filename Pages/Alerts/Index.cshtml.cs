using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaSys.Models;
using PharmaSys.Services;

namespace PharmaSys.Pages.Alerts
{
    public class IndexModel : PageModel
    {
        private readonly AlertService _alerts;
        public IndexModel(AlertService alerts) => _alerts = alerts;

        public List<Medication> LowStock { get; set; } = new();
        public List<Medication> Expiring { get; set; } = new();

        public async Task OnGetAsync()
        {
            LowStock = await _alerts.GetLowStockAsync();
            Expiring = await _alerts.GetExpiringAsync(30);
        }
    }
}
