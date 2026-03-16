# 🛍️ ShopNest — Full Stack E-Commerce Application

![ShopNest](https://img.shields.io/badge/ShopNest-E--Commerce-blue?style=for-the-badge)
![Angular](https://img.shields.io/badge/Angular-15-red?style=for-the-badge&logo=angular)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-.NET_8-purple?style=for-the-badge&logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-orange?style=for-the-badge&logo=microsoftsqlserver)

A fully featured e-commerce web application built from scratch using **Angular 15**, **ASP.NET Core .NET 8**, **Dapper**, and **SQL Server**.

---

## 🚀 Features

### 👤 Customer
- Browse & search products with filters
- Filter by category
- Add to cart with quantity selector
- Checkout & place orders
- View order history with expand/collapse
- Filter orders by status
- Search orders by product or category name
- Contact admin via Contact Us form
- Dark / Light mode toggle

### 🛠️ Admin
- Dashboard with key metrics (users, orders, revenue)
- Add, edit, soft-delete products
- View all orders with search & status filter
- Update order status
- View orders grouped by user
- Delete users (with all their data)
- Dark / Light mode toggle

### 📧 Email Notifications
- Order confirmation email sent to customer after placing order
- Contact Us form sends message directly to admin email

---

## 🏗️ Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 15, Bootstrap 5, ngx-toastr |
| Backend | ASP.NET Core .NET 8 Web API |
| Data Access | Dapper (raw SQL) |
| Database | SQL Server (SQLEXPRESS) |
| Auth | JWT + BCrypt password hashing |
| Email | MailKit (Gmail SMTP) |

---

## 📁 Project Structure

```
ShopNest/
├── ShopNest.API/               → ASP.NET Core Web API
│   ├── Controllers/            → API endpoints
│   ├── Services/               → Business logic
│   ├── Repositories/           → Database queries (Dapper)
│   ├── DTOs/                   → Data transfer objects
│   ├── Models/                 → Database models
│   └── Data/                   → Dapper context
│
├── ShopNest.Client/            → Angular 15 Frontend
│   └── src/app/
│       ├── core/
│       │   ├── services/       → API services
│       │   ├── guards/         → Auth & Admin guards
│       │   └── interceptors/   → JWT interceptor
│       ├── features/           → Components
│       │   ├── dashboard/      → Admin dashboard
│       │   ├── products/       → Product list, form
│       │   ├── cart/           → Shopping cart
│       │   ├── order/          → Checkout, my orders, admin orders
│       │   ├── auth/           → Login, register
│       │   ├── contact/        → Contact us form
│       │   └── user-orders/    → Admin user orders view
│       └── shared/models/      → TypeScript interfaces
│
└── ShopNestDB/
    ├── schema.sql              → Database tables
    └── seed.sql                → Sample data
```

---

## 🗄️ Database Tables

```
Categories     → Product categories
Products       → Products with stock, images, soft delete
Users          → Customers and Admins
Carts          → One cart per user
CartItems      → Items in cart (CASCADE delete)
Orders         → Customer orders with status
OrderItems     → Items in each order (CASCADE delete)
```

---

## ⚙️ Setup Instructions

### Prerequisites
- Node.js 18+
- .NET 8 SDK
- SQL Server (SQLEXPRESS)
- Visual Studio 2022
- VS Code

---

### 1. Database Setup

Open **SSMS** and run:

```sql
-- Step 1: Run schema
-- Open ShopNestDB/schema.sql and execute

-- Step 2: Run seed data
-- Open ShopNestDB/seed.sql and execute
```

---

### 2. Backend Setup

Open `ShopNest.API` in **Visual Studio**

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=ShopNestDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "ShopNest.API",
    "Audience": "ShopNest.Client",
    "ExpiryInDays": 7
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "YOUR_GMAIL@gmail.com",
    "SenderName": "ShopNest",
    "Password": "YOUR_GMAIL_APP_PASSWORD"
  }
}
```

Run the API:
```bash
dotnet run
# API runs at https://localhost:7271
```

---

### 3. Frontend Setup

Open `ShopNest.Client` in **VS Code**

```bash
npm install
ng serve
# App runs at http://localhost:4200
```

> To create a new admin — register normally then run:
> ```sql
> UPDATE Users SET Role = 'Admin' WHERE Email = 'your@email.com'
> ```

---

## 📸 Pages

| Page | Route | Access |
|---|---|---|
| Dashboard | `/dashboard` |	Admin |
| Products | `/products` | Public |
| Product Form | `/products/new` | Admin |
| Cart | `/cart` | Customer |
| Checkout | `/checkout` | Customer |
| My Orders | `/my-orders` | Customer |
| Contact Us | `/contact` | Customer |
| Admin Orders | `/admin-orders` | Admin |
| User Orders | `/user-orders` | Admin |
| Login | `/login` | Public |
| Register | `/register` | Public |

---

## 📦 Key Packages

### Backend
```
MailKit
BCrypt.Net-Next
Dapper
System.IdentityModel.Tokens.Jwt
Microsoft.AspNetCore.Authentication.JwtBearer
```

### Frontend
```
ngx-toastr@16
bootstrap@5
bootstrap-icons
@angular/animations
```

---

## 🌙 Dark Mode

ShopNest supports dark/light mode toggle — preference is saved in `localStorage` and persists across sessions.

---

## 📄 License

This project is open source and available under the [MIT License](LICENSE).
