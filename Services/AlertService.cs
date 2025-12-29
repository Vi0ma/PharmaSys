using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Services
{
    public class AlertService
    {
        private readonly PharmaSysDbContext _db;
        public AlertService(PharmaSysDbContext db) => _db = db;

        public async Task<int> GetAlertsCountAsync(int daysBeforeExpire = 30)
        {
            var today = DateTime.Today;
            var limit = today.AddDays(daysBeforeExpire);

            var lowStockCount = await _db.Medications.CountAsync(m => m.IsActive && m.Stock <= m.StockMin);
            var expCount = await _db.Medications.CountAsync(m => m.IsActive && m.ExpirationDate != null && m.ExpirationDate <= limit);

            return lowStockCount + expCount;
        }

        public async Task<List<Medication>> GetLowStockAsync()
        {
            return await _db.Medications
                .Include(m => m.Category)
                .Where(m => m.IsActive && m.Stock <= m.StockMin)
                .OrderBy(m => m.Stock)
                .ToListAsync();
        }

        public async Task<List<Medication>> GetExpiringAsync(int daysBeforeExpire = 30)
        {
            var today = DateTime.Today;
            var limit = today.AddDays(daysBeforeExpire);

            return await _db.Medications
                .Include(m => m.Category)
                .Where(m => m.IsActive && m.ExpirationDate != null && m.ExpirationDate <= limit)
                .OrderBy(m => m.ExpirationDate)
                .ToListAsync();
        }
    }
}
