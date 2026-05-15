USE AviahCollectionDB;
GO

-- Create ProductionBatches table to track manufacturing progress
IF OBJECT_ID('dbo.ProductionBatches', 'U') IS NOT NULL DROP TABLE dbo.ProductionBatches;
GO

CREATE TABLE ProductionBatches (
    BatchID NVARCHAR(50) PRIMARY KEY, -- PB-001
    ProductID INT FOREIGN KEY REFERENCES Products(ProductID),
    TargetQuantity INT NOT NULL,
    ProducedQuantity INT DEFAULT 0,
    Defects INT DEFAULT 0,
    StartDate DATETIME DEFAULT GETDATE(),
    EndDate DATETIME,
    Status NVARCHAR(50) DEFAULT 'Planned' -- Planned, In Progress, Quality Check, Completed
);
GO

-- Insert some initial production data
DECLARE @Prod1 INT, @Prod2 INT;
SELECT TOP 1 @Prod1 = ProductID FROM Products WHERE ProductName LIKE '%Dress%';
SELECT TOP 1 @Prod2 = ProductID FROM Products WHERE ProductName LIKE '%Blouse%';

INSERT INTO ProductionBatches (BatchID, ProductID, TargetQuantity, ProducedQuantity, Status) VALUES 
('PB-1001', @Prod1, 200, 120, 'In Progress'),
('PB-1002', @Prod2, 150, 45, 'In Progress'),
('PB-1003', @Prod1, 100, 100, 'Quality Check');
GO

PRINT 'ProductionBatches table created and seeded.';
