$conn = New-Object System.Data.SqlClient.SqlConnection("Server=REVISION-PC\SQLEXPRESS;Database=AviahCollectionDB;Trusted_Connection=True;Encrypt=False;")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "
    -- Insert a few Raw Materials if they don't exist
    IF NOT EXISTS (SELECT 1 FROM RawMaterials WHERE MaterialID = 'MAT-100')
        INSERT INTO RawMaterials (MaterialID, ItemName, Category, Quantity, Unit, Status) VALUES ('MAT-100', 'Premium Linen', 'Fabric', 500, 'meters', 'In Stock')
    IF NOT EXISTS (SELECT 1 FROM RawMaterials WHERE MaterialID = 'MAT-101')
        INSERT INTO RawMaterials (MaterialID, ItemName, Category, Quantity, Unit, Status) VALUES ('MAT-101', 'Silk Thread (Gold)', 'Thread', 200, 'pcs', 'In Stock')
    IF NOT EXISTS (SELECT 1 FROM RawMaterials WHERE MaterialID = 'MAT-102')
        INSERT INTO RawMaterials (MaterialID, ItemName, Category, Quantity, Unit, Status) VALUES ('MAT-102', 'Cotton Fabric (Red)', 'Fabric', 300, 'meters', 'In Stock')
    IF NOT EXISTS (SELECT 1 FROM RawMaterials WHERE MaterialID = 'MAT-103')
        INSERT INTO RawMaterials (MaterialID, ItemName, Category, Quantity, Unit, Status) VALUES ('MAT-103', 'Zippers (YKK)', 'Accessories', 1000, 'pcs', 'In Stock')

    -- Insert new Products
    INSERT INTO Products (ProductName, Category, StockQuantity, Price, ImageUrl)
    VALUES 
    ('Crimson Silk Evening Gown', 'Dress', 15, 4500.00, 'dress.png'),
    ('Classic Linen Trousers', 'Pants', 25, 1200.00, 'pants.png'),
    ('Golden Thread Blouse', 'Top', 40, 850.00, 'top.png');

    -- Get the last 3 inserted ProductIDs
    DECLARE @P1 INT, @P2 INT, @P3 INT;
    SET @P3 = IDENT_CURRENT('Products');
    SET @P2 = @P3 - 1;
    SET @P1 = @P3 - 2;

    -- Now link them in ProductMaterials
    INSERT INTO ProductMaterials (ProductID, MaterialID, QuantityRequired, Unit)
    VALUES 
    (@P1, 'MAT-102', 3.5, 'meters'),
    (@P1, 'MAT-101', 2, 'pcs'),
    
    (@P2, 'MAT-100', 2.0, 'meters'),
    (@P2, 'MAT-103', 1, 'pcs'),

    (@P3, 'MAT-100', 1.5, 'meters'),
    (@P3, 'MAT-101', 1, 'pcs');
"
$cmd.ExecuteNonQuery()
$conn.Close()
Write-Host "Data Seeded"
