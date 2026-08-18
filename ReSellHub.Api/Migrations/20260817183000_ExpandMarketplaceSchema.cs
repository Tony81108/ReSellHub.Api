using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ReSellHub.Api.Data;

#nullable disable

namespace ReSellHub.Api.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260817183000_ExpandMarketplaceSchema")]
public class ExpandMarketplaceSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE [Categories] (
                [Id] int NOT NULL IDENTITY,
                [Name] nvarchar(80) NOT NULL,
                [Slug] nvarchar(80) NOT NULL,
                [Icon] nvarchar(10) NOT NULL,
                [SortOrder] int NOT NULL,
                [IsActive] bit NOT NULL,
                CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
            );
            CREATE UNIQUE INDEX [IX_Categories_Slug] ON [Categories] ([Slug]);

            CREATE TABLE [Users] (
                [Id] int NOT NULL IDENTITY,
                [DisplayName] nvarchar(100) NOT NULL,
                [Email] nvarchar(255) NOT NULL,
                [PasswordHash] nvarchar(500) NOT NULL,
                [Phone] nvarchar(30) NOT NULL,
                [Role] nvarchar(20) NOT NULL,
                [IsActive] bit NOT NULL,
                [CreatedAt] datetime2 NOT NULL,
                CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
            );
            CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

            ALTER TABLE [Products] ADD
                [CategoryId] int NULL,
                [SellerId] int NULL,
                [Description] nvarchar(2000) NOT NULL CONSTRAINT [DF_Products_Description] DEFAULT N'',
                [Condition] nvarchar(30) NOT NULL CONSTRAINT [DF_Products_Condition] DEFAULT N'二手良品',
                [CoverImageUrl] nvarchar(500) NULL,
                [IsActive] bit NOT NULL CONSTRAINT [DF_Products_IsActive] DEFAULT CAST(1 AS bit),
                [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_Products_CreatedAt] DEFAULT GETUTCDATE(),
                [UpdatedAt] datetime2 NOT NULL CONSTRAINT [DF_Products_UpdatedAt] DEFAULT GETUTCDATE();
            EXEC(N'CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId])');
            EXEC(N'CREATE INDEX [IX_Products_SellerId] ON [Products] ([SellerId])');
            EXEC(N'ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE SET NULL');
            EXEC(N'ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_Users_SellerId] FOREIGN KEY ([SellerId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL');

            CREATE TABLE [ProductImages] (
                [Id] int NOT NULL IDENTITY, [ProductId] int NOT NULL, [ImageUrl] nvarchar(500) NOT NULL,
                [IsPrimary] bit NOT NULL, [SortOrder] int NOT NULL,
                CONSTRAINT [PK_ProductImages] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_ProductImages_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
            );
            CREATE INDEX [IX_ProductImages_ProductId] ON [ProductImages] ([ProductId]);

            CREATE TABLE [Addresses] (
                [Id] int NOT NULL IDENTITY, [UserId] int NOT NULL, [RecipientName] nvarchar(60) NOT NULL,
                [Phone] nvarchar(30) NOT NULL, [PostalCode] nvarchar(20) NOT NULL, [City] nvarchar(40) NOT NULL,
                [District] nvarchar(40) NOT NULL, [Street] nvarchar(300) NOT NULL, [IsDefault] bit NOT NULL,
                CONSTRAINT [PK_Addresses] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_Addresses_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
            );
            CREATE INDEX [IX_Addresses_UserId] ON [Addresses] ([UserId]);

            CREATE TABLE [ShoppingCarts] (
                [Id] int NOT NULL IDENTITY, [UserId] int NOT NULL, [UpdatedAt] datetime2 NOT NULL,
                CONSTRAINT [PK_ShoppingCarts] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_ShoppingCarts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX [IX_ShoppingCarts_UserId] ON [ShoppingCarts] ([UserId]);

            CREATE TABLE [CartItems] (
                [Id] int NOT NULL IDENTITY, [CartId] int NOT NULL, [ProductId] int NOT NULL,
                [Quantity] int NOT NULL, [AddedAt] datetime2 NOT NULL,
                CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_CartItems_ShoppingCarts_CartId] FOREIGN KEY ([CartId]) REFERENCES [ShoppingCarts] ([Id]) ON DELETE CASCADE,
                CONSTRAINT [FK_CartItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX [IX_CartItems_CartId_ProductId] ON [CartItems] ([CartId], [ProductId]);
            CREATE INDEX [IX_CartItems_ProductId] ON [CartItems] ([ProductId]);

            CREATE TABLE [Favorites] (
                [Id] int NOT NULL IDENTITY, [UserId] int NOT NULL, [ProductId] int NOT NULL, [CreatedAt] datetime2 NOT NULL,
                CONSTRAINT [PK_Favorites] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_Favorites_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
                CONSTRAINT [FK_Favorites_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX [IX_Favorites_UserId_ProductId] ON [Favorites] ([UserId], [ProductId]);
            CREATE INDEX [IX_Favorites_ProductId] ON [Favorites] ([ProductId]);

            CREATE TABLE [Orders] (
                [Id] int NOT NULL IDENTITY, [OrderNumber] nvarchar(30) NOT NULL, [UserId] int NOT NULL,
                [Status] nvarchar(30) NOT NULL, [PaymentStatus] nvarchar(30) NOT NULL, [ShippingStatus] nvarchar(30) NOT NULL,
                [Subtotal] decimal(18,2) NOT NULL, [ShippingFee] decimal(18,2) NOT NULL, [TotalAmount] decimal(18,2) NOT NULL,
                [RecipientName] nvarchar(60) NOT NULL, [RecipientPhone] nvarchar(30) NOT NULL,
                [ShippingAddress] nvarchar(500) NOT NULL, [CreatedAt] datetime2 NOT NULL,
                CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_Orders_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
            );
            CREATE UNIQUE INDEX [IX_Orders_OrderNumber] ON [Orders] ([OrderNumber]);
            CREATE INDEX [IX_Orders_UserId] ON [Orders] ([UserId]);

            CREATE TABLE [OrderItems] (
                [Id] int NOT NULL IDENTITY, [OrderId] int NOT NULL, [ProductId] int NULL,
                [ProductName] nvarchar(200) NOT NULL, [UnitPrice] decimal(18,2) NOT NULL,
                [Quantity] int NOT NULL, [LineTotal] decimal(18,2) NOT NULL,
                CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
                CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE SET NULL
            );
            CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
            CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);

            SET IDENTITY_INSERT [Categories] ON;
            INSERT INTO [Categories] ([Id],[Name],[Slug],[Icon],[SortOrder],[IsActive]) VALUES
            (1,N'手機平板',N'mobile',N'📱',1,1),(2,N'電腦周邊',N'computer',N'💻',2,1),
            (3,N'居家生活',N'home',N'🏠',3,1),(4,N'服飾配件',N'fashion',N'👕',4,1),
            (5,N'書籍影音',N'books',N'📚',5,1),(6,N'運動戶外',N'sports',N'🏃',6,1),
            (7,N'其他好物',N'others',N'📦',7,1);
            SET IDENTITY_INSERT [Categories] OFF;
            """);

        // CategoryId 必須在上一個 SQL 批次完成建立後，才能更新既有商品。
        migrationBuilder.Sql("UPDATE [Products] SET [CategoryId] = 7 WHERE [CategoryId] IS NULL;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE [OrderItems]; DROP TABLE [Orders]; DROP TABLE [Favorites]; DROP TABLE [CartItems];
            DROP TABLE [ShoppingCarts]; DROP TABLE [Addresses]; DROP TABLE [ProductImages];
            ALTER TABLE [Products] DROP CONSTRAINT [FK_Products_Categories_CategoryId];
            ALTER TABLE [Products] DROP CONSTRAINT [FK_Products_Users_SellerId];
            DROP INDEX [IX_Products_CategoryId] ON [Products]; DROP INDEX [IX_Products_SellerId] ON [Products];
            ALTER TABLE [Products] DROP CONSTRAINT [DF_Products_Description], [DF_Products_Condition], [DF_Products_IsActive], [DF_Products_CreatedAt], [DF_Products_UpdatedAt];
            ALTER TABLE [Products] DROP COLUMN [CategoryId],[SellerId],[Description],[Condition],[CoverImageUrl],[IsActive],[CreatedAt],[UpdatedAt];
            DROP TABLE [Categories]; DROP TABLE [Users];
            """);
    }
}
