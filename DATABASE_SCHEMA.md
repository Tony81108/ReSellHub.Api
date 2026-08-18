# ReSellHub 資料庫架構

目前共 10 張資料表：

1. `Products`：商品主檔，連結分類與賣家。
2. `Categories`：商品分類。
3. `ProductImages`：一項商品可有多張圖片。
4. `Users`：買家與賣家帳號核心資料（密碼只存雜湊）。
5. `Addresses`：會員收件地址。
6. `ShoppingCarts`：會員購物車。
7. `CartItems`：購物車商品明細。
8. `Favorites`：會員收藏商品。
9. `Orders`：訂單主檔與收件快照。
10. `OrderItems`：訂單商品與成交價格快照。

## 關聯重點

- Category 1 → N Products
- User 1 → N Products / Addresses / Orders
- User 1 → 1 ShoppingCart
- ShoppingCart 1 → N CartItems
- Product 1 → N ProductImages
- Order 1 → N OrderItems
- User N ↔ N Products（透過 Favorites）

## 啟動

開發環境第一次按 F5 時，程式會自動執行 EF Core Migration，保留現有商品並建立新資料表。正式環境建議改由部署流程執行 Migration。
