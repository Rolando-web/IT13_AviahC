USE AviahCollectionDB;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DELETE FROM Feedback;
    DELETE FROM OrderItems;
    DELETE FROM Deliveries;
    DELETE FROM PurchaseOrders;
    DELETE FROM Orders;
    DELETE FROM RawMaterials;
    DELETE FROM Products;
    DELETE FROM Promotions;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
