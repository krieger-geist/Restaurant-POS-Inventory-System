-- ============================================================
-- Restaurant POS — Complete SQL Server Setup Script
-- Visual Studio 2026 | VB.NET | .NET 10 | net10.0-windows
-- Run in SQL Server Management Studio or Azure Data Studio
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'RestaurantPOSDB')
BEGIN
    CREATE DATABASE RestaurantPOSDB;
    PRINT 'Database RestaurantPOSDB created.';
END
GO

USE RestaurantPOSDB;
GO

-- Users
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
CREATE TABLE Users (
    UserID       INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role         NVARCHAR(50)  NOT NULL DEFAULT 'Staff',
    FullName     NVARCHAR(200),
    Email        NVARCHAR(200),
    Phone        NVARCHAR(50),
    IsActive     BIT NOT NULL DEFAULT 1,
    LastLogin    DATETIME,
    CreatedDate  DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Menu Categories
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MenuCategories' AND xtype='U')
CREATE TABLE MenuCategories (
    CategoryID  INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Description NVARCHAR(300),
    SortOrder   INT DEFAULT 0,
    IsActive    BIT DEFAULT 1
);
GO

-- Menu Items
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MenuItems' AND xtype='U')
CREATE TABLE MenuItems (
    ItemID      INT IDENTITY(1,1) PRIMARY KEY,
    CategoryID  INT FOREIGN KEY REFERENCES MenuCategories(CategoryID) ON DELETE SET NULL,
    Name        NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    Price       DECIMAL(10,2) NOT NULL DEFAULT 0,
    TaxPercent  DECIMAL(5,2)  DEFAULT 5,
    IsVeg       BIT DEFAULT 1,
    IsAvailable BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);
CREATE INDEX IX_MenuItems_Category ON MenuItems(CategoryID);
GO

-- Restaurant Tables
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RestaurantTables' AND xtype='U')
CREATE TABLE RestaurantTables (
    TableID      INT IDENTITY(1,1) PRIMARY KEY,
    TableNumber  NVARCHAR(20) NOT NULL UNIQUE,
    Capacity     INT DEFAULT 4,
    Location     NVARCHAR(100),
    Status       NVARCHAR(20) NOT NULL DEFAULT 'Available'
);
GO

-- Staff
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Staff' AND xtype='U')
CREATE TABLE Staff (
    StaffID     INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(200) NOT NULL,
    Phone       NVARCHAR(50),
    Email       NVARCHAR(200),
    Role        NVARCHAR(100),
    Department  NVARCHAR(100),
    Salary      DECIMAL(10,2) DEFAULT 0,
    JoinDate    DATE,
    IsActive    BIT DEFAULT 1,
    UserID      INT FOREIGN KEY REFERENCES Users(UserID) ON DELETE SET NULL,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Customers
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Customers' AND xtype='U')
CREATE TABLE Customers (
    CustomerID    INT IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(200) NOT NULL,
    Phone         NVARCHAR(50),
    Email         NVARCHAR(200),
    Address       NVARCHAR(500),
    LoyaltyPoints INT DEFAULT 0,
    TotalVisits   INT DEFAULT 0,
    TotalSpent    DECIMAL(10,2) DEFAULT 0,
    CreatedDate   DATETIME DEFAULT GETDATE()
);
GO

-- Orders
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' AND xtype='U')
CREATE TABLE Orders (
    OrderID         INT IDENTITY(1,1) PRIMARY KEY,
    TableID         INT FOREIGN KEY REFERENCES RestaurantTables(TableID) ON DELETE SET NULL,
    CustomerID      INT FOREIGN KEY REFERENCES Customers(CustomerID) ON DELETE SET NULL,
    CustomerName    NVARCHAR(200),
    OrderType       NVARCHAR(50)  NOT NULL DEFAULT 'Dine-In',
    Status          NVARCHAR(50)  NOT NULL DEFAULT 'Open',
    SubTotal        DECIMAL(10,2) DEFAULT 0,
    TaxAmount       DECIMAL(10,2) DEFAULT 0,
    DiscountPercent DECIMAL(5,2)  DEFAULT 0,
    DiscountAmount  DECIMAL(10,2) DEFAULT 0,
    TotalAmount     DECIMAL(10,2) NOT NULL DEFAULT 0,
    PaymentMode     NVARCHAR(50),
    PaidAmount      DECIMAL(10,2) DEFAULT 0,
    ChangeAmount    DECIMAL(10,2) DEFAULT 0,
    Notes           NVARCHAR(500),
    OrderDate       DATETIME NOT NULL DEFAULT GETDATE(),
    ClosedDate      DATETIME,
    CreatedBy       INT FOREIGN KEY REFERENCES Users(UserID) ON DELETE SET NULL
);
CREATE INDEX IX_Orders_Date   ON Orders(OrderDate);
CREATE INDEX IX_Orders_Status ON Orders(Status);
GO

-- Order Items
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrderItems' AND xtype='U')
CREATE TABLE OrderItems (
    OrderItemID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID     INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderID) ON DELETE CASCADE,
    ItemID      INT FOREIGN KEY REFERENCES MenuItems(ItemID) ON DELETE SET NULL,
    ItemName    NVARCHAR(200),
    Quantity    INT NOT NULL DEFAULT 1,
    UnitPrice   DECIMAL(10,2) NOT NULL,
    TaxPercent  DECIMAL(5,2)  DEFAULT 0,
    TotalPrice  DECIMAL(10,2) NOT NULL,
    Notes       NVARCHAR(200),
    KOTSent     BIT DEFAULT 0
);
CREATE INDEX IX_OrderItems_OrderID ON OrderItems(OrderID);
GO

-- Reservations
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reservations' AND xtype='U')
CREATE TABLE Reservations (
    ReservationID   INT IDENTITY(1,1) PRIMARY KEY,
    CustomerName    NVARCHAR(200) NOT NULL,
    Phone           NVARCHAR(50),
    TableID         INT FOREIGN KEY REFERENCES RestaurantTables(TableID) ON DELETE SET NULL,
    GuestCount      INT DEFAULT 1,
    ReservationDate DATETIME NOT NULL,
    Status          NVARCHAR(50) DEFAULT 'Confirmed',
    Notes           NVARCHAR(500),
    CreatedDate     DATETIME DEFAULT GETDATE(),
    CreatedBy       INT FOREIGN KEY REFERENCES Users(UserID) ON DELETE SET NULL
);
CREATE INDEX IX_Reservations_Date ON Reservations(ReservationDate);
GO

-- Inventory
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Inventory' AND xtype='U')
CREATE TABLE Inventory (
    InventoryID  INT IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(200) NOT NULL,
    Category     NVARCHAR(100),
    Unit         NVARCHAR(50),
    CurrentStock DECIMAL(10,2) DEFAULT 0,
    MinStock     DECIMAL(10,2) DEFAULT 5,
    CostPerUnit  DECIMAL(10,2) DEFAULT 0,
    Supplier     NVARCHAR(200),
    LastUpdated  DATETIME DEFAULT GETDATE()
);
GO

-- Expenses
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Expenses' AND xtype='U')
CREATE TABLE Expenses (
    ExpenseID   INT IDENTITY(1,1) PRIMARY KEY,
    Category    NVARCHAR(100),
    Description NVARCHAR(500),
    Amount      DECIMAL(10,2) NOT NULL,
    ExpenseDate DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    PaidTo      NVARCHAR(200),
    CreatedBy   INT FOREIGN KEY REFERENCES Users(UserID) ON DELETE SET NULL,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Activity Log
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ActivityLog' AND xtype='U')
CREATE TABLE ActivityLog (
    LogID    INT IDENTITY(1,1) PRIMARY KEY,
    UserID   INT FOREIGN KEY REFERENCES Users(UserID) ON DELETE SET NULL,
    Username NVARCHAR(100),
    Action   NVARCHAR(200),
    Details  NVARCHAR(1000),
    LogDate  DATETIME DEFAULT GETDATE()
);
GO

-- ============================================================
-- USEFUL VIEWS
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.views WHERE name='vw_DailySales')
EXEC('CREATE VIEW vw_DailySales AS
SELECT CAST(ClosedDate AS DATE) AS SaleDate,
    COUNT(*) AS TotalOrders,
    SUM(TotalAmount) AS Revenue,
    AVG(TotalAmount) AS AvgOrder,
    SUM(TaxAmount) AS TaxCollected
FROM Orders WHERE Status=''Closed''
GROUP BY CAST(ClosedDate AS DATE)');
GO

IF NOT EXISTS (SELECT * FROM sys.views WHERE name='vw_MenuPerformance')
EXEC('CREATE VIEW vw_MenuPerformance AS
SELECT mi.Name AS MenuItem, mc.Name AS Category,
    ISNULL(SUM(oi.Quantity),0) AS TotalSold,
    ISNULL(SUM(oi.TotalPrice),0) AS Revenue
FROM MenuItems mi
LEFT JOIN MenuCategories mc ON mi.CategoryID=mc.CategoryID
LEFT JOIN OrderItems oi ON mi.ItemID=oi.ItemID
LEFT JOIN Orders o ON oi.OrderID=o.OrderID AND o.Status=''Closed''
GROUP BY mi.Name, mc.Name');
GO

-- ============================================================
-- STORED PROCEDURES
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_DailyReport')
EXEC('CREATE PROCEDURE sp_DailyReport @Date DATE = NULL AS
BEGIN
    IF @Date IS NULL SET @Date = CAST(GETDATE() AS DATE);
    SELECT COUNT(*) AS Orders, ISNULL(SUM(TotalAmount),0) AS Revenue,
        ISNULL(AVG(TotalAmount),0) AS AvgOrder
    FROM Orders WHERE Status=''Closed'' AND CAST(ClosedDate AS DATE)=@Date;
END');
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_LowInventory')
EXEC('CREATE PROCEDURE sp_LowInventory AS
BEGIN
    SELECT Name, Category, CurrentStock, MinStock,
        CASE WHEN CurrentStock=0 THEN ''OUT OF STOCK'' ELSE ''LOW STOCK'' END AS Alert
    FROM Inventory WHERE CurrentStock<=MinStock ORDER BY CurrentStock ASC;
END');
GO

-- Verify
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_CATALOG='RestaurantPOSDB' ORDER BY TABLE_NAME;
PRINT 'Setup complete! Restaurant POS database ready.';
GO
