# ReSellHub（二手商品交易商城）

ReSellHub 是一個以 **ASP.NET Core 8 Web API、Vue 3 與 SQL Server** 建立的二手商品交易平台。專案參考電商商城的使用流程，整合商品瀏覽、分類篩選、會員驗證、購物車、結帳及賣家商品管理，作為前後端整合與資料庫設計的 Side Project。

## 專案畫面

> 建議將商城首頁截圖存放於 `docs/images/store-home.png`，再取消下一行的註解。

<!-- ![ReSellHub 商城首頁](docs/images/store-home.png) -->

## 核心功能

### 買家商城

- 商品清單與商品詳情
- 關鍵字搜尋及商品分類篩選
- 商品庫存與狀態顯示
- 響應式側邊分類導覽

### 會員系統

- 會員註冊、登入及登出
- Cookie Authentication 身分驗證
- API 未登入狀態回傳 HTTP 401
- 會員登入狀態查詢

### 購物車與結帳

- 加入購物車
- 修改商品數量
- 移除購物車商品
- 庫存及商品有效性檢查
- 建立訂單與訂單明細

### 賣家管理

- 商品新增、查詢、修改及刪除
- 商品分類、價格、庫存與上下架管理
- ASP.NET Core MVC 管理頁面

## 使用技術

| 類別 | 技術 |
|---|---|
| Backend | C#、ASP.NET Core 8、Web API、MVC |
| Frontend | Vue 3、Vite、HTML5、CSS3、JavaScript |
| Database | SQL Server、Entity Framework Core 8、Code First Migration |
| Authentication | ASP.NET Core Cookie Authentication |
| API 文件 | Swagger / OpenAPI |
| Tools | Visual Studio 2022、Git、GitHub、npm |

## 系統架構

```text
Vue 3 Frontend
      │ HTTP / JSON
      ▼
ASP.NET Core Web API ── Cookie Authentication
      │
      ▼
Entity Framework Core
      │
      ▼
SQL Server
```

## 專案結構

```text
ReSellHub.Api/
├─ Controllers/       # 商城、會員、購物車、結帳與商品 API
├─ Data/              # AppDbContext 與測試資料 Seeder
├─ Migrations/        # EF Core 資料庫版本管理
├─ Models/            # Entity 與 View Model
├─ Views/             # MVC 賣家商品管理頁面
├─ ClientApp/         # Vue 3 原始碼
├─ wwwroot/app/       # Vue 正式版建置檔案
├─ Program.cs         # DI、Middleware 與路由設定
└─ appsettings.json   # 應用程式設定
```

## 資料庫設計

主要資料表包括：

- `Users`：會員與賣家資料
- `Categories`：商品分類
- `Products`：商品、價格、庫存及上下架狀態
- `ProductImages`：商品圖片
- `Addresses`：會員收件地址
- `Carts`、`CartItems`：購物車及明細
- `Orders`、`OrderItems`：訂單及訂單明細

詳細欄位與關聯請參考 [DATABASE_SCHEMA.md](DATABASE_SCHEMA.md)。

## 主要 API

| Method | Endpoint | 功能 |
|---|---|---|
| GET | `/api/store/categories` | 取得商品分類 |
| GET | `/api/store/products` | 搜尋及篩選商品 |
| GET | `/api/store/products/{id}` | 取得商品詳情 |
| POST | `/api/auth/register` | 會員註冊 |
| POST | `/api/auth/login` | 會員登入 |
| POST | `/api/auth/logout` | 會員登出 |
| GET | `/api/cart` | 取得購物車 |
| POST | `/api/cart/items` | 加入購物車 |
| PUT | `/api/cart/items/{itemId}` | 修改商品數量 |
| DELETE | `/api/cart/items/{itemId}` | 移除購物車商品 |
| POST | `/api/checkout` | 建立訂單 |

## 開發環境

- Visual Studio 2022
- .NET 8 SDK
- SQL Server / SQL Server Express
- Node.js 20 以上版本
- npm

## 啟動方式

### 1. 取得專案

```bash
git clone https://github.com/Tony81108/ReSellHub.Api.git
cd ReSellHub.Api
```

### 2. 設定資料庫

在 `ReSellHub.Api/appsettings.json` 設定自己的 SQL Server 連線字串。請勿將正式環境密碼或 API Key 提交至 GitHub。

### 3. 建立資料庫

在 Visual Studio 的「套件管理器主控台」執行：

```powershell
Update-Database
```

Development 環境啟動時也會執行 Migration，並透過 `DbSeeder` 建立示範資料。

### 4. 啟動後端

使用 Visual Studio 2022 開啟 `ReSellHub.Api.sln`，將 `ReSellHub.Api` 設為啟始專案後按 `F5`。

Swagger 開發文件：

```text
https://localhost:{port}/swagger
```

### 5. 啟動 Vue 開發環境（選用）

```bash
cd ReSellHub.Api/ClientApp
npm install
npm run dev
```

建立正式版前端：

```bash
npm run build
```

## 設計重點

- 以 Controller、Model、Data 層劃分責任
- 使用 Dependency Injection 管理 `AppDbContext`
- 使用 EF Core Migration 管理資料庫結構版本
- 使用關聯式資料表與外鍵維持資料完整性
- 結帳流程於後端重新驗證商品狀態、價格及庫存
- Vue 前端透過 RESTful API 與 ASP.NET Core 後端溝通

## 後續規劃

- JWT 或 OAuth 第三方登入
- 商品圖片上傳與雲端儲存
- 金流及物流服務串接
- 訂單狀態與歷程追蹤
- 收藏、評價與賣家中心
- 自動化測試及 CI/CD
- Docker 容器化與雲端部署

## 作者

Tony Lu（呂東陵）

- GitHub：[@Tony81108](https://github.com/Tony81108)

