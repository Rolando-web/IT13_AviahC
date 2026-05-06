-- SQL Script to create Users table for AviahCollectionDB
-- Connection: REVISION-PC\SQLEXPRESS

USE AviahCollectionDB;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) UNIQUE NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL, -- Recommended for security
        Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Superadmin', 'Admin', 'Customer', 'Staff', 'DeliverStaff', 'Supplier')),
        CreatedAt DATETIME DEFAULT GETDATE()
    );

    -- Insert a default admin for testing
    INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role)
    VALUES ('System', 'Admin', 'admin@aviah.com', 'admin123', 'Admin');
END
GO
