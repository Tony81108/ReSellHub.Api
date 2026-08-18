using Microsoft.EntityFrameworkCore;
using ReSellHub.Api.Models;

namespace ReSellHub.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
        modelBuilder.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Order>().HasIndex(x => x.OrderNumber).IsUnique();
        modelBuilder.Entity<CartItem>().HasIndex(x => new { x.CartId, x.ProductId }).IsUnique();
        modelBuilder.Entity<Favorite>().HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
        modelBuilder.Entity<ShoppingCart>().HasIndex(x => x.UserId).IsUnique();

        modelBuilder.Entity<Product>().HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Product>().HasOne(x => x.Seller).WithMany(x => x.Products).HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<ProductImage>().HasOne(x => x.Product).WithMany(x => x.Images).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Address>().HasOne(x => x.User).WithMany(x => x.Addresses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ShoppingCart>().HasOne(x => x.User).WithMany(x => x.Carts).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CartItem>().HasOne(x => x.Cart).WithMany(x => x.Items).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CartItem>().HasOne(x => x.Product).WithMany(x => x.CartItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Favorite>().HasOne(x => x.User).WithMany(x => x.Favorites).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Favorite>().HasOne(x => x.Product).WithMany(x => x.Favorites).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Order>().HasOne(x => x.User).WithMany(x => x.Orders).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<OrderItem>().HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OrderItem>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "手機平板", Slug = "mobile", Icon = "📱", SortOrder = 1 },
            new Category { Id = 2, Name = "電腦周邊", Slug = "computer", Icon = "💻", SortOrder = 2 },
            new Category { Id = 3, Name = "居家生活", Slug = "home", Icon = "🏠", SortOrder = 3 },
            new Category { Id = 4, Name = "服飾配件", Slug = "fashion", Icon = "👕", SortOrder = 4 },
            new Category { Id = 5, Name = "書籍影音", Slug = "books", Icon = "📚", SortOrder = 5 },
            new Category { Id = 6, Name = "運動戶外", Slug = "sports", Icon = "🏃", SortOrder = 6 },
            new Category { Id = 7, Name = "其他好物", Slug = "others", Icon = "📦", SortOrder = 7 });
    }
}
