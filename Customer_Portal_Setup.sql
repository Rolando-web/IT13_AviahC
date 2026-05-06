-- SQL Script for Customer Portal tables (Products version)
USE AviahCollectionDB;
GO

-- Create Products table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        ProductID INT PRIMARY KEY IDENTITY(1,1),
        ProductName NVARCHAR(100) NOT NULL,
        Category NVARCHAR(50),
        StockQuantity INT DEFAULT 0,
        Price DECIMAL(18,2),
        ImageUrl NVARCHAR(255),
        Description NVARCHAR(MAX)
    );
END
GO

-- Create Orders table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
BEGIN
    CREATE TABLE Orders (
        OrderID INT PRIMARY KEY IDENTITY(1,1),
        OrderRef NVARCHAR(50) UNIQUE,
        UserId INT FOREIGN KEY REFERENCES Users(Id),
        OrderDate DATETIME DEFAULT GETDATE(),
        TotalAmount DECIMAL(18,2),
        Status NVARCHAR(50) DEFAULT 'Pending',
        ItemSummary NVARCHAR(MAX)
    );
END
GO

-- Seed some Products
IF (SELECT COUNT(*) FROM Products) = 0
BEGIN
    INSERT INTO Products (ProductName, Category, StockQuantity, Price, ImageUrl, Description)
    VALUES 
    ('Violet Summer Dress', 'Dresses', 25, 120.00, 'dress.png', 'A beautiful violet summer dress.'),
    ('Lavender Blouse', 'Tops', 40, 65.00, 'shirt.png', 'Light and airy lavender blouse.'),
    ('Plum Formal Trousers', 'Pants', 15, 85.00, 'pants.png', 'Elegant plum formal trousers.'),
    ('Silk Scarf (Lilac)', 'Accessories', 50, 35.00, 'hoodie.png', 'Premium silk scarf in lilac.'),
    ('Purple Cardigan', 'Outerwear', 20, 55.00, 'longsleeve.png', 'Warm and cozy purple cardigan.');
END
GO

-- Ensure there is a customer for testing My Orders
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'customer@aviah.com')
BEGIN
    INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role)
    VALUES ('Sarah', 'Johnson', 'customer@aviah.com', 'customer123', 'Customer');
END
GO

-- Seed some Orders for testing
DECLARE @CustomerId INT;
SELECT TOP 1 @CustomerId = Id FROM Users WHERE Email = 'customer@aviah.com';

IF @CustomerId IS NOT NULL AND (SELECT COUNT(*) FROM Orders) = 0
BEGIN
    INSERT INTO Orders (OrderRef, UserId, OrderDate, TotalAmount, Status, ItemSummary)
    VALUES 
    ('ORD-983', @CustomerId, GETDATE(), 185.00, 'In Transit', 'Lavender Blouse, Trousers'),
    ('ORD-842', @CustomerId, DATEADD(day, -5, GETDATE()), 35.00, 'Delivered', 'Silk Scarf (Lilac)'),
    ('ORD-710', @CustomerId, DATEADD(day, -10, GETDATE()), 120.00, 'Delivered', 'Violet Summer Dress');
END
GO
