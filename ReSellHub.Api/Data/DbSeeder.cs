using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ReSellHub.Api.Models;

namespace ReSellHub.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var existingDemo = await context.Users.FirstOrDefaultAsync(user => user.Email == "demo@resellhub.local");
        if (existingDemo is not null && existingDemo.PasswordHash == "LOGIN_NOT_ENABLED")
        {
            existingDemo.PasswordHash = new PasswordHasher<AppUser>().HashPassword(existingDemo, "Demo123!");
            await context.SaveChangesAsync();
        }

        var demoAlreadyExists = await context.Users.AnyAsync(user => user.Email == "demo@resellhub.local")
            && await context.Products.AnyAsync(product => product.Name == "iPhone 14 128GB 二手機");

        if (demoAlreadyExists)
        {
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync();

        var categories = await context.Categories.ToDictionaryAsync(category => category.Slug);
        if (categories.Count == 0)
        {
            return;
        }

        var demoUser = await context.Users.FirstOrDefaultAsync(user => user.Email == "demo@resellhub.local");
        if (demoUser is null)
        {
            demoUser = new AppUser
            {
                DisplayName = "ReSellHub 體驗會員",
                Email = "demo@resellhub.local",
                PasswordHash = "LOGIN_NOT_ENABLED",
                Phone = "0900000000",
                Role = "Buyer"
            };
            context.Users.Add(demoUser);
            demoUser.PasswordHash = new PasswordHasher<AppUser>().HashPassword(demoUser, "Demo123!");
            await context.SaveChangesAsync();

            context.Addresses.Add(new Address
            {
                UserId = demoUser.Id,
                RecipientName = "體驗會員",
                Phone = "0900000000",
                PostalCode = "100",
                City = "臺北市",
                District = "中正區",
                Street = "展示地址（非真實資料）",
                IsDefault = true
            });
        }

        var now = DateTime.UtcNow;
        var products = new List<Product>
        {
            NewProduct(categories["mobile"].Id, "iPhone 14 128GB 二手機", 12000, 16800, 1, "近全新", "功能正常，外觀保存良好，附充電線。", now),
            NewProduct(categories["mobile"].Id, "平板電腦 10 吋 Wi-Fi 版", 3200, 4990, 2, "二手良品", "適合追劇與閱讀，螢幕無明顯刮痕。", now),
            NewProduct(categories["computer"].Id, "機械式鍵盤 青軸", 650, 990, 3, "二手良品", "按鍵功能正常，已完成清潔。", now),
            NewProduct(categories["computer"].Id, "27 吋 IPS 電腦螢幕", 2800, 4200, 1, "近全新", "色彩顯示正常，附原廠電源線。", now),
            NewProduct(categories["home"].Id, "北歐風桌燈", 280, 450, 4, "二手良品", "暖色燈光，適合書桌與床頭使用。", now),
            NewProduct(categories["home"].Id, "小型氣炸鍋 3L", 900, 1380, 1, "使用痕跡明顯", "加熱功能正常，內鍋有正常使用痕跡。", now),
            NewProduct(categories["fashion"].Id, "經典帆布後背包", 380, 690, 2, "近全新", "容量充足，可放 13 吋筆電。", now),
            NewProduct(categories["fashion"].Id, "中性防風外套 M 號", 520, 880, 1, "二手良品", "少穿，拉鍊與口袋功能正常。", now),
            NewProduct(categories["books"].Id, "C# 程式設計入門套書", 400, 650, 2, "二手良品", "內頁乾淨，適合 .NET 初學者。", now),
            NewProduct(categories["books"].Id, "商業管理精選書籍 5 本", 300, 500, 1, "二手良品", "五本合售，書況良好。", now),
            NewProduct(categories["sports"].Id, "瑜珈墊 6mm", 180, 320, 5, "近全新", "止滑效果良好，已清潔。", now),
            NewProduct(categories["sports"].Id, "慢跑腰包與水壺組", 220, 390, 3, "全新未使用", "適合路跑與日常訓練。", now)
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        context.ProductImages.AddRange(products.Select(product => new ProductImage
        {
            ProductId = product.Id,
            ImageUrl = "/images/product-placeholder.svg",
            IsPrimary = true
        }));

        var cart = new ShoppingCart { UserId = demoUser.Id, UpdatedAt = now };
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        context.CartItems.AddRange(
            new CartItem { CartId = cart.Id, ProductId = products[2].Id, Quantity = 1, AddedAt = now },
            new CartItem { CartId = cart.Id, ProductId = products[10].Id, Quantity = 2, AddedAt = now });
        context.Favorites.Add(new Favorite { UserId = demoUser.Id, ProductId = products[0].Id, CreatedAt = now });

        var order = new Order
        {
            OrderNumber = $"DEMO-{now:yyyyMMdd}-001",
            UserId = demoUser.Id,
            Status = "Completed",
            PaymentStatus = "Paid",
            ShippingStatus = "Delivered",
            Subtotal = products[8].SellingPrice,
            ShippingFee = 60,
            TotalAmount = products[8].SellingPrice + 60,
            RecipientName = "體驗會員",
            RecipientPhone = "0900000000",
            ShippingAddress = "100 臺北市中正區展示地址（非真實資料）",
            CreatedAt = now.AddDays(-7)
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        context.OrderItems.Add(new OrderItem
        {
            OrderId = order.Id,
            ProductId = products[8].Id,
            ProductName = products[8].Name,
            UnitPrice = products[8].SellingPrice,
            Quantity = 1,
            LineTotal = products[8].SellingPrice
        });

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private static Product NewProduct(int categoryId, string name, decimal cost, decimal price, int stock, string condition, string description, DateTime now)
        => new()
        {
            CategoryId = categoryId,
            Name = name,
            CostPrice = cost,
            SellingPrice = price,
            StockQuantity = stock,
            Condition = condition,
            Description = description,
            CoverImageUrl = "/images/product-placeholder.svg",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
}
