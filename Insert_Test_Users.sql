-- SQL Script to insert test accounts for AviahCollectionDB
USE AviahCollectionDB;
GO

-- Insert Superadmin
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role)
VALUES ('Super', 'Admin', 'superadmin@aviah.com', 'super123', 'Superadmin');

-- Insert Staff
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role)
VALUES ('Office', 'Staff', 'staff@aviah.com', 'staff123', 'Staff');

-- Insert Delivery Staff (Note: matching 'DeliverStaff' from schema check constraint)
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role)
VALUES ('Rider', 'One', 'delivery@aviah.com', 'rider123', 'DeliverStaff');

-- Insert Customer
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role)
VALUES ('Sarah', 'Johnson', 'customer@aviah.com', 'customer123', 'Customer');

-- Insert Supplier
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role)
VALUES ('Textile', 'Solutions', 'supplier@aviah.com', 'supplier123', 'Supplier');

GO

-- Verify the insertions
SELECT * FROM Users;
