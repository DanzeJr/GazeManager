using GazeManager.Models;
using Microsoft.EntityFrameworkCore;

namespace GazeManager.Services
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Configuration> Configuration { get; set; }

        public DbSet<User> User { get; set; }

        public DbSet<ProductOption> ProductOption { get; set; }

        public DbSet<Product> Product { get; set; }

        public DbSet<Category> Category { get; set; }

        public DbSet<Order> Order { get; set; }

        public DbSet<DeviceInfo> DeviceInfo { get; set; }

        public DbSet<CartItem> Cart { get; set; }

        public DbSet<ProductImage> ProductImage { get; set; }

        public DbSet<ProductOrder> ProductOrder { get; set; }

        public DbSet<ProductCategory> ProductCategory { get; set; }

        public DbSet<Notification> Notification { get; set; }

        public DbSet<Wishlist> Wishlist { get; set; }

        public DbSet<NewsInfo> News { get; set; }
    }
}