using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;

namespace PharmaSys.Pages.Clients
{
    public class DeleteModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public DeleteModel(PharmaSysDbContext db) => _db = db;

        public Client Client { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var c = await _db.Clients.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return RedirectToPage("Index");
            Client = c;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var c = await _db.Clients.FirstOrDefaultAsync(x => x.Id == id);
            if (c != null)
            {
                _db.Clients.Remove(c);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage("Index");
        }
    }
}
