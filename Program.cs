using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Services;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Razor Pages + Identity
// =======================
builder.Services.AddRazorPages(options =>
{
    // ✅ Autoriser Identity sans login (OBLIGATOIRE)
    options.Conventions.AllowAnonymousToAreaFolder("Identity", "/Account");
});

// =======================
// DbContext
// =======================
builder.Services.AddDbContext<PharmaSysDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// =======================
// Identity
// =======================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<PharmaSysDbContext>()
.AddDefaultTokenProviders();

// =======================
// Cookies
// =======================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// =======================
// Services
// =======================
builder.Services.AddScoped<AlertService>();

// =======================
// Authorization globale
// =======================
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// =======================
// Pipeline HTTP
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// =======================
// SEED DATA (APRÈS BUILD)
// =======================
using (var scope = app.Services.CreateScope())
{
    await SeedData.SeedAsync(scope.ServiceProvider);
}

app.Run();
