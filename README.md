# 🍽 Restaurant POS v1.0
### Professional Restaurant Billing & Management System
**VB.NET | Visual Studio 2026 (18.6.2) | .NET 10 (net10.0-windows) | SQL Server LocalDB**

Developed by **Manas Suryavanshi, Yash Parashar, and Abhay Mahawar**


---

## ✅ Compatible With Your Setup
| Your System | This Project |
|---|---|
| Visual Studio 2026 v18.6.2 | ✅ Fully compatible |
| .NET SDK 10.0.300 | ✅ Targets net10.0-windows |
| VB.NET Windows Forms App | ✅ Uses WinForms template |
| SQL Server LocalDB | ✅ Auto-creates DB on first run |

---

## 📁 Project Structure
```
RestaurantPOS/
├── RestaurantPOS.vbproj          ← net10.0-windows, no LiveCharts, NoWarn NU1701
├── Program.vb                    ← Entry point
│
├── Database/
│   ├── DatabaseManager.vb        ← All DB operations + auto-init
│   └── RestaurantPOS_Setup.sql   ← Manual SQL Server setup script
│
├── Forms/
│   ├── FrmLogin.vb               ← BCrypt login form
│   └── FrmDashboard.vb           ← Main window with sidebar nav
│
├── Helpers/
│   └── AppTheme.vb               ← Colors, fonts, SessionManager, StyleBtn/StyleGrid
│
└── Modules/
    ├── BillingModule.vb          ← POS billing, KOT print, PDF bill
    ├── TableModule.vb            ← Visual table layout + CRUD
    ├── MenuModule.vb             ← Menu items + category management
    ├── OrdersModule.vb           ← Order history with filters
    ├── CombinedModules.vb        ← ReservationModule + CustomerModule + StaffModule
    └── AdminModules.vb           ← InventoryModule + ExpensesModule + ReportsModule
                                     + UserModule + SettingsModule
```

---

## 🚀 Build & Run

### In Visual Studio 2026
```
1. Open RestaurantPOS.vbproj
2. Build → Build Solution  (Ctrl+Shift+B)
3. Press F5 to run
```

### Command Line (PowerShell)
```powershell
cd E:\your\path\RestaurantPOS
dotnet restore
dotnet build
dotnet run
```

---

## 🔐 Default Login Credentials

| Role | Username | Password |
|---|---|---|
| **Admin** | `admin` | `Admin@123` |
| **Manager** | `manager` | `Manager@123` |
| **Staff** | `staff` | `Staff@123` |

> ⚠ Change passwords after first login via **Settings → Change My Password**

---

## 📦 NuGet Packages (Auto-restored)

| Package | Purpose |
|---|---|
| `System.Data.SqlClient 4.8.6` | SQL Server database access |
| `BCrypt.Net-Next 4.0.3` | Secure password hashing |
| `iTextSharp 5.5.13.3` | PDF bill & report generation |
| `NPOI 2.7.0` | Excel (.xlsx) report export |

The NU1701 warnings about .NET Framework compatibility are **suppressed** in the project file and are harmless — all packages work correctly at runtime.

---

## 🍽 Module Guide

### 🧾 Billing & POS
- Select table, order type (Dine-In/Takeaway/Delivery/Counter), customer name
- Double-click any menu item to add it to the order
- Discount %, payment mode, amount paid → auto-calculates change
- **Save Order** — keeps order open (can reload from "Open Orders" panel)
- **Close & Pay** — finalises bill, updates table status, offers PDF
- **KOT Print** — Kitchen Order Ticket print preview
- **PDF Bill** — professional A5 tax invoice exported via iTextSharp

### 🪑 Table Manager
- Visual colour-coded grid: 🟢 Available | 🔴 Occupied | 🟠 Reserved
- Click any table card to load its details into the edit form
- Add / edit / delete tables with location and capacity

### 🍴 Menu Manager
- Full CRUD for menu items with category, price, tax%, veg/non-veg flag
- Toggle availability (keeps item in DB but hides from billing)
- Separate **Categories** dialog to add/remove categories
- Search and filter by category

### 📋 Orders
- Filter by date range, status (Open/Closed), type, or customer name
- Click any order row to see its items in the lower grid
- Right-click context menu → Mark Closed / Cancel Order
- Today / This Month shortcut buttons

### 📅 Reservations
- Future / past reservation management with table assignment
- Date + time picker, guest count, status tracking

### 👥 Customers
- Full customer records with loyalty points, visit count, total spent
- Auto-loads purchase history from Orders table

### 👤 Staff
- CRUD for kitchen, floor, billing, and management staff
- Salary tracking, department, join date

### 📦 Inventory
- Stock level tracking with low-stock highlighting (red rows)
- Min stock alert button to filter only low items
- Cost per unit and supplier tracking

### 💰 Expenses
- Log daily expenses by category (Rent, Salary, Utility, etc.)
- Live "This Month Total" counter

### 📊 Reports
- 4 views: Order Summary | Menu Performance | Revenue Chart | (export)
- Custom date range or Today/This Month presets
- **Excel Export** (NPOI) — two sheets: orders + menu performance
- **PDF Export** (iTextSharp) — landscape A4 report with header stats
- Bar chart rendered with GDI+ (no third-party chart library needed)

### 🔑 User Accounts *(Admin only)*
- Create/edit/deactivate system users
- Assign roles: Admin / Manager / Staff
- **Activity Log** tab — full audit trail

### ⚙ Settings
- Live DB connection string editor + test button
- Database backup to `.bak` file
- Change your own password (BCrypt re-hash)
- System info display

---

## 🗄 Database Notes

The app **auto-creates** the `RestaurantPOSDB` database using SQL Server LocalDB on first launch — no manual setup needed.

To use a different SQL Server instance, go to **Settings → Database** and update the connection string, then click **Update** and **Test Connection**.

For manual setup, run `Database/RestaurantPOS_Setup.sql` in SSMS.

---

## 🔒 Security Features
- BCrypt password hashing (cost factor 11) — industry standard
- Role-based access (User Accounts module requires Admin)
- Full activity log for all CRUD operations
- Session tracking with login timestamp

---


## 👥 Project Contributors

This project was developed collaboratively by a team, with each member contributing to the design, development, testing, and overall implementation of the Restaurant POS System.

* **Manas Suryavanshi** – Project Development, Application Architecture, Backend Logic, Database Integration, and GitHub Repository Management.
* **Yash Parashar** – Feature Development, UI Implementation, Testing, and Bug Fixes.
* **Abhay Mahawar** – Module Development, Database Support, Documentation, and Quality Assurance.

We worked together using GitHub for version control, coordinated development efforts, and ensured seamless integration of all modules to build a complete Restaurant POS & Inventory Management System...



