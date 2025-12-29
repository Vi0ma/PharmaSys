using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Models;

namespace PharmaSys.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<PharmaSysDbContext>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // ✅ Assure que la base existe (Identity + tables)
            await db.Database.MigrateAsync();

            // =======================
            // 1️⃣ RÔLES
            // =======================
            string[] roles = { "Admin", "Pharmacien", "Caissier" };

            foreach (var role in roles)
            {
                if (!await roleMgr.RoleExistsAsync(role))
                {
                    await roleMgr.CreateAsync(new IdentityRole(role));
                }
            }

            // =======================
            // 2️⃣ ADMIN PAR DÉFAUT
            // =======================
            string adminEmail = "admin@pharmasys.local";

            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Admin Pharmacie",
                    RoleLabel = "Administrateur"
                };

                var result = await userMgr.CreateAsync(admin, "Admin@12345");

                if (result.Succeeded)
                {
                    await userMgr.AddToRoleAsync(admin, "Admin");
                }
            }

            // =======================
            // 3️⃣ CATÉGORIES
            // =======================
            if (!await db.Categories.AnyAsync())
            {
                db.Categories.AddRange(
                    new Category { Name = "Analgésique" },
                    new Category { Name = "Antibiotique" },
                    new Category { Name = "Anti-inflammatoire" },
                    new Category { Name = "Antiagrégant" }
                );
                await db.SaveChangesAsync();
            }

            // =======================
            // 4️⃣ FOURNISSEURS
            // =======================
            if (!await db.Suppliers.AnyAsync())
            {
                db.Suppliers.AddRange(
                    new Supplier
                    {
                        Name = "PharmaSupply",
                        Phone = "0612345678",
                        Email = "contact@pharmasupply.com",
                        Address = "Casablanca"
                    },
                    new Supplier
                    {
                        Name = "MediCorp",
                        Phone = "0600000000",
                        Email = "sales@medicorp.com",
                        Address = "Rabat"
                    }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}
