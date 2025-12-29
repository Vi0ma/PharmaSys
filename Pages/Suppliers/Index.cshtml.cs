using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Suppliers
{
    [Authorize(Roles = "Admin,Pharmacien")]
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public IndexModel(PharmaSysDbContext db) => _db = db;

        public List<Supplier> Suppliers { get; set; } = new();

        public async Task OnGetAsync()
        {
            Suppliers = await _db.Suppliers
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }
}
