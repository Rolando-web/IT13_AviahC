-- Switch to the correct database
USE AviahCollectionDB;
GO

-- DROP existing tables to ensure they are recreated with the correct columns
IF EXISTS (SELECT * FROM sysobjects WHERE name='Promotions' AND xtype='U')
    DROP TABLE Promotions;
GO

IF EXISTS (SELECT * FROM sysobjects WHERE name='Deliveries' AND xtype='U')
    DROP TABLE Deliveries;
GO

-- Create Promotions table with the NEW schema
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
GO

-- Create Deliveries table with the NEW schema
CREATE TABLE Deliveries (
    DeliveryID NVARCHAR(50) PRIMARY KEY,
    OrderRef NVARCHAR(50),
    CustomerName NVARCHAR(100),
    Status NVARCHAR(50),
    ETA NVARCHAR(50),
    DriverName NVARCHAR(100),
    CurrentLocation NVARCHAR(200),
    Destination NVARCHAR(200),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- Insert sample data
INSERT INTO Promotions (PromoCode, PromotionName, DiscountValue, TargetAudience, Status, UsageCount, MaxUsage, StartDate, EndDate)
VALUES 
('SUMMER24', 'Summer Sale 2024', '20% OFF', 'All Customers', 'Active', 850, 1000, '2024-06-01', '2024-08-31'),
('NEWUSER', 'New User Discount', '₱500 OFF', 'First-time Buyers', 'Active', 1200, 2000, '2024-01-01', '2024-12-31'),
('FLASHACCESS', 'Flash Sale: Accessories', '50% OFF', 'Selected Items', 'Inactive', 500, 500, '2024-04-01', '2024-05-01'),
('LOYALTY', 'Loyalty Reward', '15% OFF', 'Premium Members', 'Active', 300, 1000, '2024-05-15', '2024-10-15');

INSERT INTO Deliveries (DeliveryID, OrderRef, CustomerName, Status, ETA, DriverName, CurrentLocation, Destination)
VALUES 
('DEL-8401', 'ORD-984', 'Jayr Luayon', 'In Transit', '45 mins', 'Mike Ross', 'Downtown Metro', '123 Main St, NY'),
('DEL-8402', 'ORD-982', 'Sarah Johnson', 'Delivered', '-', 'Sarah Connor', 'Uptown', '456 Park Ave, NY'),
('DEL-8403', 'ORD-990', 'Mike Chen', 'Pending', 'TBD', 'Unassigned', 'Warehouse A', '789 Broadway, NY'),
('DEL-8404', 'ORD-995', 'Anna Belle', 'Pending', 'TBD', 'Unassigned', 'Warehouse B', '321 Pine St, CA');
GO
