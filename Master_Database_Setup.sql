USE AviahCollectionDB;
GO

-- 1. Drop existing tables to start fresh
-- Drop tables with foreign keys first
IF OBJECT_ID('dbo.DeliveryHistory', 'U') IS NOT NULL DROP TABLE dbo.DeliveryHistory;
IF OBJECT_ID('dbo.Feedback', 'U') IS NOT NULL DROP TABLE dbo.Feedback;
IF OBJECT_ID('dbo.OrderItems', 'U') IS NOT NULL DROP TABLE dbo.OrderItems;
IF OBJECT_ID('dbo.Deliveries', 'U') IS NOT NULL DROP TABLE dbo.Deliveries;
IF OBJECT_ID('dbo.PurchaseOrders', 'U') IS NOT NULL DROP TABLE dbo.PurchaseOrders;
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.RawMaterials', 'U') IS NOT NULL DROP TABLE dbo.RawMaterials;
IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Promotions', 'U') IS NOT NULL DROP TABLE dbo.Promotions;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- 2. Create Tables

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Superadmin', 'Admin', 'Customer', 'Staff', 'DeliverStaff', 'Supplier')),
    SubscriptionTier NVARCHAR(20) DEFAULT 'Basic' CHECK (SubscriptionTier IN ('Basic', 'Standard', 'Premium')),
    CompanyId INT NULL,
    IsLoggedIn BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- For tracking active sessions (Tier 3 feature)
CREATE TABLE UserSessions (
    SessionId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    LoginTime DATETIME DEFAULT GETDATE(),
    LastActivity DATETIME DEFAULT GETDATE(),
    DeviceType NVARCHAR(50)
);

CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50),
    StockQuantity INT DEFAULT 0,
    Price DECIMAL(18,2),
    ImageUrl NVARCHAR(255),
    Description NVARCHAR(MAX)
);

CREATE TABLE Promotions (
    PromoID INT PRIMARY KEY IDENTITY(1,1),
    PromoCode NVARCHAR(50),
    PromotionName NVARCHAR(100),
    DiscountValue NVARCHAR(50),
    TargetAudience NVARCHAR(100),
    Status NVARCHAR(50),
    UsageCount INT DEFAULT 0,
    MaxUsage INT DEFAULT 1000,
    StartDate DATE,
    EndDate DATE
);

CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    OrderRef NVARCHAR(50) UNIQUE,
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2),
    Status NVARCHAR(150) DEFAULT 'Pending',
    ItemSummary NVARCHAR(MAX)
);

CREATE TABLE Deliveries (
    DeliveryID NVARCHAR(50) PRIMARY KEY,
    OrderRef NVARCHAR(50) FOREIGN KEY REFERENCES Orders(OrderRef),
    DriverID INT FOREIGN KEY REFERENCES Users(Id) NULL,
    CustomerName NVARCHAR(100),
    Status NVARCHAR(150),
    ETA NVARCHAR(50),
    DriverName NVARCHAR(100),
    Destination     NVARCHAR(200),
    CreatedAt       DATETIME DEFAULT GETDATE()
);

CREATE TABLE DeliveryHistory (
    HistoryID INT IDENTITY(1,1) PRIMARY KEY,
    DeliveryID NVARCHAR(50) FOREIGN KEY REFERENCES Deliveries(DeliveryID) ON DELETE CASCADE,
    Status NVARCHAR(150),
    Location NVARCHAR(200),
    UpdateTime DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE RawMaterials (
    MaterialID NVARCHAR(50) PRIMARY KEY, -- SKU like RM-001
    ItemName NVARCHAR(100),
    Category NVARCHAR(50),
    Quantity INT,
    Unit NVARCHAR(20),
    Status NVARCHAR(50), -- In Stock, Low Stock, Critical
    ImageUrl NVARCHAR(255)
);

CREATE TABLE PurchaseOrders (
    PONumber NVARCHAR(50) PRIMARY KEY,
    SupplierID INT FOREIGN KEY REFERENCES Users(Id),
    MaterialID NVARCHAR(50) FOREIGN KEY REFERENCES RawMaterials(MaterialID),
    Quantity INT,
    DueDate DATE,
    Status NVARCHAR(50) -- Pending, In Production, Ready
);

CREATE TABLE Feedback (
    FeedbackID INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    OrderRef NVARCHAR(50) FOREIGN KEY REFERENCES Orders(OrderRef),
    Rating INT CHECK (Rating >= 1 AND Rating <= 5),
    Comments NVARCHAR(MAX),
    DateSubmitted DATETIME DEFAULT GETDATE()
);
GO

-- 3. Insert Base Data
-- Users
-- Users with Tiers
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role, SubscriptionTier, CompanyId) VALUES 
('Tier1', 'Admin', 't1@aviah.com', 'admin123', 'Admin', 'Basic', 1),
('Tier2', 'Admin', 't2@aviah.com', 'admin123', 'Admin', 'Standard', 1),
('Tier3', 'Admin', 't3@aviah.com', 'admin123', 'Admin', 'Premium', 1),
('System', 'Admin', 'admin@aviah.com', 'admin123', 'Admin', 'Basic', 1),
('Super', 'Admin', 'superadmin@aviah.com', 'super123', 'Superadmin', 'Premium', NULL),
('Office', 'Staff', 'staff@aviah.com', 'staff123', 'Staff', 'Standard', 1),
('Rider', 'One', 'delivery@aviah.com', 'rider123', 'DeliverStaff', 'Premium', 1),
('Sarah', 'Johnson', 'customer@aviah.com', 'customer123', 'Customer', 'Premium', NULL),
('Textile', 'Solutions', 'supplier@aviah.com', 'supplier123', 'Supplier', 'Standard', 1);

-- Products
INSERT INTO Products (ProductName, Category, StockQuantity, Price, ImageUrl, Description) VALUES 
('Violet Summer Dress', 'Dresses', 25, 120.00, 'dress.png', 'A beautiful violet summer dress.'),
('Lavender Blouse', 'Tops', 40, 65.00, 'shirt.png', 'Light and airy lavender blouse.'),
('Plum Formal Trousers', 'Pants', 15, 85.00, 'pants.png', 'Elegant plum formal trousers.'),
('Silk Scarf (Lilac)', 'Accessories', 50, 35.00, 'hoodie.png', 'Premium silk scarf in lilac.'),
('Purple Cardigan', 'Outerwear', 20, 55.00, 'longsleeve.png', 'Warm and cozy purple cardigan.');

-- Promotions
INSERT INTO Promotions (PromoCode, PromotionName, DiscountValue, TargetAudience, Status, UsageCount, MaxUsage, StartDate, EndDate) VALUES 
('SUMMER24', 'Summer Sale 2024', '20% OFF', 'All Customers', 'Active', 850, 1000, '2024-06-01', '2024-08-31'),
('NEWUSER', 'New User Discount', 'P500 OFF', 'First-time Buyers', 'Active', 1200, 2000, '2024-01-01', '2024-12-31'),
('FLASHACCESS', 'Flash Sale: Accessories', '50% OFF', 'Selected Items', 'Inactive', 500, 500, '2024-04-01', '2024-05-01'),
('LOYALTY', 'Loyalty Reward', '15% OFF', 'Premium Members', 'Active', 300, 1000, '2024-05-15', '2024-10-15');

-- Orders
DECLARE @CustomerId INT;
SELECT TOP 1 @CustomerId = Id FROM Users WHERE Email = 'customer@aviah.com';

INSERT INTO Orders (OrderRef, UserId, OrderDate, TotalAmount, Status, ItemSummary) VALUES 
('ORD-984', @CustomerId, GETDATE(), 185.00, 'In Transit', 'Lavender Blouse, Trousers'),
('ORD-982', @CustomerId, DATEADD(day, -5, GETDATE()), 35.00, 'Delivered', 'Silk Scarf (Lilac)'),
('ORD-990', @CustomerId, DATEADD(day, -10, GETDATE()), 120.00, 'Pending', 'Violet Summer Dress'),
('ORD-1290', @CustomerId, GETDATE(), 250.00, 'In Transit', 'Purple Cardigan, Silk Scarf'),
('ORD-1285', @CustomerId, GETDATE(), 65.00, 'Pending', 'Lavender Blouse'),
('ORD-1282', @CustomerId, GETDATE(), 120.00, 'Pending', 'Violet Summer Dress');

-- Deliveries
INSERT INTO Deliveries (DeliveryID, OrderRef, CustomerName, Status, ETA, DriverName, CurrentLocation, Destination) VALUES 
('DEL-8401', 'ORD-984', 'Jayr Luayon', 'In Transit', '45 mins', 'Mike Ross', 'Downtown Metro', '123 Main St, NY'),
('DEL-8402', 'ORD-982', 'Sarah Johnson', 'Delivered', '-', 'Sarah Connor', 'Uptown', '456 Park Ave, NY'),
('DEL-8403', 'ORD-990', 'Mike Chen', 'Pending', 'TBD', 'Unassigned', 'Warehouse A', '789 Broadway, NY');

-- Raw Materials
INSERT INTO RawMaterials (MaterialID, ItemName, Category, Quantity, Unit, Status) VALUES 
('RM-001', 'Premium Cotton Fabric', 'Fabric', 450, 'meters', 'In Stock'),
('RM-045', 'Silk Thread (Purple)', 'Thread', 20, 'pcs', 'Low Stock'),
('RM-102', 'Buttons (Gold, 12mm)', 'Accessories', 1500, 'pcs', 'In Stock'),
('RM-022', 'Denim Fabric', 'Fabric', 5, 'meters', 'Critical');

-- Purchase Orders
DECLARE @SupplierId INT;
SELECT TOP 1 @SupplierId = Id FROM Users WHERE Email = 'supplier@aviah.com';

INSERT INTO PurchaseOrders (PONumber, SupplierID, MaterialID, Quantity, DueDate, Status) VALUES 
('PO-8821', @SupplierId, 'RM-045', 50, '2024-04-25', 'In Production'),
('PO-8815', @SupplierId, 'RM-001', 120, '2024-04-28', 'Pending'),
('PO-8798', @SupplierId, 'RM-102', 30, '2024-04-22', 'Ready');

GO
