USE AviahCollectionDB;
GO

-- Add the missing ImageUrl column to the RawMaterials table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RawMaterials') AND name = 'ImageUrl')
BEGIN
    ALTER TABLE dbo.RawMaterials ADD ImageUrl NVARCHAR(255);
    PRINT 'Added ImageUrl column to RawMaterials table.';
END
ELSE
BEGIN
    PRINT 'ImageUrl column already exists in RawMaterials table.';
END
GO
