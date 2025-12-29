using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Models;
using System.Globalization;
using System.Text;

namespace PharmaSys.Pages.Medications
{
    public class IndexModel : PageModel
    {
        private readonly PharmaSysDbContext _db;
        public IndexModel(PharmaSysDbContext db) => _db = db;

        public List<Medication> Medications { get; set; } = new();

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        [BindProperty] public IFormFile? CsvFile { get; set; }

        public async Task OnGetAsync()
        {
            Medications = await _db.Medications
                .Include(m => m.Category)
                .Include(m => m.Supplier)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        // ============================
        // ✅ EXPORT CSV
        // ============================
        public async Task<IActionResult> OnPostExportAsync()
        {
            var meds = await _db.Medications
                .Include(m => m.Category)
                .Include(m => m.Supplier)
                .OrderBy(m => m.Name)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Code;Name;Dosage;Manufacturer;Barcode;Category;Stock;StockMin;PurchasePrice;SalePrice;ExpirationDate;Location;Supplier");

            foreach (var m in meds)
            {
                sb.AppendLine(string.Join(";",
                    Safe(m.Code),
                    Safe(m.Name),
                    Safe(m.Dosage),
                    Safe(m.Manufacturer),
                    Safe(m.Barcode),
                    Safe(m.Category?.Name),
                    m.Stock.ToString(),
                    m.StockMin.ToString(),
                    m.PurchasePrice.ToString(CultureInfo.InvariantCulture),
                    m.SalePrice.ToString(CultureInfo.InvariantCulture),
                    m.ExpirationDate?.ToString("yyyy-MM-dd") ?? "",
                    Safe(m.Location),
                    Safe(m.Supplier?.Name)
                ));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv; charset=utf-8", $"medications_{DateTime.Now:yyyyMMdd_HHmm}.csv");

            static string Safe(string? s) => (s ?? "").Replace(";", " ").Replace("\n", " ").Replace("\r", " ");
        }

        // ============================
        // ✅ TEMPLATE CSV
        // ============================
        public IActionResult OnPostTemplate()
        {
            var template =
@"Code;Name;Dosage;Manufacturer;Barcode;Category;Stock;StockMin;PurchasePrice;SalePrice;ExpirationDate;Location;Supplier
MED001;Paracétamol;500mg;PharmaSupply;1234567890123;Analgésique;150;50;18;25;2025-12-31;A1-B2;PharmaSupply
MED002;Amoxicilline;1g;MediCorp;9876543210123;Antibiotique;45;20;60;85;2025-08-15;B-01;MediCorp
";
            var bytes = Encoding.UTF8.GetBytes(template);
            return File(bytes, "text/csv; charset=utf-8", "template_medications.csv");
        }

        // ============================
        // ✅ IMPORT CSV (AJOUT/UPDATE)
        // ============================
        public async Task<IActionResult> OnPostImportAsync()
        {
            if (CsvFile == null || CsvFile.Length == 0)
            {
                ErrorMessage = "Veuillez choisir un fichier CSV.";
                return RedirectToPage();
            }

            if (!Path.GetExtension(CsvFile.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Le fichier doit être au format .csv.";
                return RedirectToPage();
            }

            int inserted = 0, updated = 0, skipped = 0;

            using var reader = new StreamReader(CsvFile.OpenReadStream(), Encoding.UTF8);

            var header = await reader.ReadLineAsync();
            if (header == null || !header.Contains("Code", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "CSV invalide (header manquant). Utilise le template CSV.";
                return RedirectToPage();
            }

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(';');
                if (parts.Length < 12)
                {
                    skipped++;
                    continue;
                }

                var code = parts[0].Trim();
                var name = parts[1].Trim();
                var dosage = parts[2].Trim();
                var manufacturer = parts[3].Trim();
                var barcode = parts[4].Trim();
                var categoryName = parts[5].Trim();
                var stockStr = parts[6].Trim();
                var stockMinStr = parts[7].Trim();
                var purchaseStr = parts[8].Trim();
                var saleStr = parts[9].Trim();
                var expStr = parts[10].Trim();
                var location = parts[11].Trim();
                var supplierName = parts.Length >= 13 ? parts[12].Trim() : "";

                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                {
                    skipped++;
                    continue;
                }

                int stock = TryInt(stockStr, 0);
                int stockMin = TryInt(stockMinStr, 0);
                decimal purchase = TryDec(purchaseStr, 0);
                decimal sale = TryDec(saleStr, 0);

                DateTime? expDate = null;
                if (!string.IsNullOrWhiteSpace(expStr))
                {
                    if (DateTime.TryParseExact(expStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        expDate = dt;
                }

                // Category (create if not exists)
                int? categoryId = null;
                if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);
                    if (cat == null)
                    {
                        cat = new Category { Name = categoryName };
                        _db.Categories.Add(cat);
                        await _db.SaveChangesAsync();
                    }
                    categoryId = cat.Id;
                }

                // Supplier (optional) - link by name if exists
                int? supplierId = null;
                if (!string.IsNullOrWhiteSpace(supplierName))
                {
                    var sup = await _db.Suppliers.FirstOrDefaultAsync(s => s.Name == supplierName);
                    if (sup != null) supplierId = sup.Id;
                }

                // Upsert by Code
                var existing = await _db.Medications.FirstOrDefaultAsync(m => m.Code == code);

                if (existing == null)
                {
                    var med = new Medication
                    {
                        Code = code,
                        Name = name,
                        Dosage = dosage,
                        Manufacturer = manufacturer,
                        Barcode = barcode,
                        CategoryId = categoryId,
                        SupplierId = supplierId,
                        Stock = stock,
                        StockMin = stockMin,
                        PurchasePrice = purchase,
                        SalePrice = sale,
                        ExpirationDate = expDate,
                        Location = location
                    };

                    _db.Medications.Add(med);
                    inserted++;
                }
                else
                {
                    existing.Name = name;
                    existing.Dosage = dosage;
                    existing.Manufacturer = manufacturer;
                    existing.Barcode = barcode;
                    existing.CategoryId = categoryId;
                    existing.SupplierId = supplierId;
                    existing.Stock = stock; // Remplacer stock (si tu veux += dis-moi)
                    existing.StockMin = stockMin;
                    existing.PurchasePrice = purchase;
                    existing.SalePrice = sale;
                    existing.ExpirationDate = expDate;
                    existing.Location = location;

                    _db.Medications.Update(existing);
                    updated++;
                }
            }

            await _db.SaveChangesAsync();
            SuccessMessage = $"Import terminé ✅ Ajoutés: {inserted}, Modifiés: {updated}, Ignorés: {skipped}";
            return RedirectToPage();

            static int TryInt(string s, int def) => int.TryParse(s, out var v) ? v : def;

            static decimal TryDec(string s, decimal def)
                => decimal.TryParse(s.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : def;
        }
    }
}
