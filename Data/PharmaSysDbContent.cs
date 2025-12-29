using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Models;

namespace PharmaSys.Data
{
    public class PharmaSysDbContext : IdentityDbContext<ApplicationUser>
    {
        public PharmaSysDbContext(DbContextOptions<PharmaSysDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Medication> Medications => Set<Medication>();
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleItem> SaleItems => Set<SaleItem>();
        public DbSet<SupplierOrder> SupplierOrders => Set<SupplierOrder>();
        public DbSet<SupplierOrderItem> SupplierOrderItems => Set<SupplierOrderItem>();
        public DbSet<Prescription> Prescriptions => Set<Prescription>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // =======================
            // SupplierOrder
            // =======================
            builder.Entity<SupplierOrder>()
                .Property(x => x.OrderNumber)
                .HasMaxLength(30)
                .IsRequired();

            builder.Entity<SupplierOrder>()
                .HasIndex(x => x.OrderNumber)
                .IsUnique();

            // =======================
            // SupplierOrderItem
            // =======================
            builder.Entity<SupplierOrderItem>()
                .Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            // =======================
            // Prescription
            // =======================
            builder.Entity<Prescription>()
                .HasIndex(x => x.Number)
                .IsUnique();

            // =======================
            // Category
            // =======================
            builder.Entity<Category>()
                .HasIndex(x => x.Name)
                .IsUnique();

            // =======================
            // Client
            // =======================
            builder.Entity<Client>()
                .HasIndex(x => x.Code)
                .IsUnique();

            // =======================
            // Medication
            // =======================
            builder.Entity<Medication>()
                .HasIndex(x => x.Code)
                .IsUnique();

            // =======================
            // Sale
            // =======================
            builder.Entity<Sale>()
                .HasIndex(x => x.InvoiceNo)
                .IsUnique();

            builder.Entity<SaleItem>()
                .HasOne(x => x.Sale)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}