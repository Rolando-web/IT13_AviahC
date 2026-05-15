USE AviahCollectionDB;
GO

-- To safely truncate the Orders table, we must first remove data from tables 
-- that have a Foreign Key relationship with it.

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Remove dependent tracking and feedback data
    DELETE FROM Feedback;
    DELETE FROM Deliveries;

    -- 2. Clear the Orders table
    -- We use DELETE instead of TRUNCATE because Orders is referenced by foreign keys
    DELETE FROM Orders;

    -- 3. Reset the identity counter so new orders start from 1 again
    DBCC CHECKIDENT ('Orders', RESEED, 0);

    COMMIT TRANSACTION;
    PRINT 'Orders table and related tracking data have been cleared successfully.';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error occurred while clearing orders: ' + ERROR_MESSAGE();
END CATCH;
GO
