using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockManagementWebApplication.Models.Entities;

namespace StockManagementWebApplication.Data
{
    public class StockManagementDbContext : IdentityDbContext<ApplicationUser>
    {
        public StockManagementDbContext(DbContextOptions<StockManagementDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            List<Item> items = new();
            items.Add(
            new Item()
            {
                Id = 1,
                Name = "Apple",
                Rate = 50,
                Quantity = 10

            });

            items.Add(
            new Item()
            {
                Id = 2,
                Name = "Orange",
                Rate = 30,
                Quantity = 10


            });
            builder.Entity<Item>().HasData(items);
            SeedRoles(builder);
            base.OnModelCreating(builder);
        }

        private void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Id = "fab4fac1-c546-41de-aebc-a14da6895711", Name = "Manager", ConcurrencyStamp = "1", NormalizedName = "Manager" },
                new IdentityRole() { Id = "c7b013f0-5201-4317-abd8-c211f91b7330", Name = "Customer", ConcurrencyStamp = "2", NormalizedName = "Customer" }
                );
        }
        public DbSet<Item> Items { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}