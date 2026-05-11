using System.Data;

namespace IT13_AviahC.Services
{
    /// <summary>
    /// Supplier-specific operations: view and record supply deliveries to warehouse.
    /// Used by: SupplierInventoryPage
    /// </summary>
    public partial class DatabaseService
    {
        /// <summary>Get all raw materials (supplier sees full warehouse list).</summary>
        public async Task<DataTable> GetWarehouseForSupplierAsync()
        {
            return await GetAllWarehouseItemsAsync();
        }

        /// <summary>
        /// Supplier records a new delivery of a raw material.
        /// Creates a PurchaseOrder and increases raw material quantity.
        /// </summary>
        public async Task<int> SupplierAddMaterialDeliveryAsync(
            string materialId, string itemName, string category,
            int quantity, string unit, string supplierEmail)
        {
            // Check if material already exists
            string checkQuery = "SELECT COUNT(*) FROM RawMaterials WHERE MaterialID = @MaterialID";
            var checkDt = await ExecuteQueryAsync(checkQuery,
                new Dictionary<string, object> { { "@MaterialID", materialId } });

            int existingCount = checkDt.Rows.Count > 0 ? Convert.ToInt32(checkDt.Rows[0][0]) : 0;

            if (existingCount == 0)
            {
                // Insert new raw material
                string insertMat = @"
                    INSERT INTO RawMaterials (MaterialID, ItemName, Category, Quantity, Unit, Status)
                    VALUES (@MaterialID, @ItemName, @Category, @Quantity, @Unit, @Status)";

                string status = quantity <= 10 ? "Critical" : quantity <= 100 ? "Low Stock" : "In Stock";
                await ExecuteNonQueryAsync(insertMat, new Dictionary<string, object>
                {
                    { "@MaterialID", materialId },
                    { "@ItemName",   itemName },
                    { "@Category",   category },
                    { "@Quantity",   quantity },
                    { "@Unit",       unit },
                    { "@Status",     status }
                });
            }
            else
            {
                // Increase existing quantity
                string updateMat = @"
                    UPDATE RawMaterials
                    SET Quantity = Quantity + @Quantity,
                        Status   = CASE
                                     WHEN (Quantity + @Quantity) <= 10 THEN 'Critical'
                                     WHEN (Quantity + @Quantity) <= 100 THEN 'Low Stock'
                                     ELSE 'In Stock'
                                   END
                    WHERE MaterialID = @MaterialID";
                await ExecuteNonQueryAsync(updateMat, new Dictionary<string, object>
                {
                    { "@Quantity",   quantity },
                    { "@MaterialID", materialId }
                });
            }

            // Create a PurchaseOrder record to track the supply
            string poNumber = "PO-" + new Random().Next(1000, 9999);
            string insertPO = @"
                INSERT INTO PurchaseOrders (PONumber, SupplierID, MaterialID, Quantity, DueDate, Status)
                VALUES (@PONumber,
                        (SELECT TOP 1 Id FROM Users WHERE Email = @Email),
                        @MaterialID, @Quantity, GETDATE(), 'Ready')";

            return await ExecuteNonQueryAsync(insertPO, new Dictionary<string, object>
            {
                { "@PONumber",    poNumber },
                { "@Email",       supplierEmail },
                { "@MaterialID",  materialId },
                { "@Quantity",    quantity }
            });
        }

        /// <summary>Update raw material quantity and status (Supplier edits an existing item).</summary>
        public async Task<int> SupplierUpdateMaterialAsync(
            string materialId, string itemName, string category,
            int quantity, string unit)
        {
            string status = quantity <= 10 ? "Critical" : quantity <= 100 ? "Low Stock" : "In Stock";
            string query = @"
                UPDATE RawMaterials
                SET ItemName  = @ItemName,
                    Category  = @Category,
                    Quantity  = @Quantity,
                    Unit      = @Unit,
                    Status    = @Status
                WHERE MaterialID = @MaterialID";

            return await ExecuteNonQueryAsync(query, new Dictionary<string, object>
            {
                { "@ItemName",   itemName },
                { "@Category",   category },
                { "@Quantity",   quantity },
                { "@Unit",       unit },
                { "@Status",     status },
                { "@MaterialID", materialId }
            });
        }

        /// <summary>Delete a raw material from the warehouse (Supplier removes an item).</summary>
        public async Task<int> SupplierDeleteMaterialAsync(string materialId)
        {
            // Remove dependent purchase orders first to avoid FK violation
            await ExecuteNonQueryAsync(
                "DELETE FROM PurchaseOrders WHERE MaterialID = @MaterialID",
                new Dictionary<string, object> { { "@MaterialID", materialId } });

            return await ExecuteNonQueryAsync(
                "DELETE FROM RawMaterials WHERE MaterialID = @MaterialID",
                new Dictionary<string, object> { { "@MaterialID", materialId } });
        }
        /// <summary>Fetch dashboard statistics for the supplier.</summary>
        public async Task<DataTable> GetSupplierDashboardStatsAsync(string supplierEmail)
        {
            // Get Supplier ID
            string userQuery = "SELECT Id FROM Users WHERE Email = @Email AND Role = 'Supplier'";
            var userDt = await ExecuteQueryAsync(userQuery, new Dictionary<string, object> { { "@Email", supplierEmail } });
            if (userDt.Rows.Count == 0) return new DataTable();
            int supplierId = (int)userDt.Rows[0]["Id"];

            // Query for stats and recent POs
            string query = @"
                SELECT 
                    (SELECT COUNT(*) FROM PurchaseOrders WHERE SupplierID = @SupplierID AND Status != 'Ready') AS ActiveOrders,
                    (SELECT COUNT(*) FROM RawMaterials WHERE Status = 'Critical') AS MaterialRequests,
                    po.PONumber,
                    rm.ItemName AS MaterialName,
                    CAST(po.Quantity AS VARCHAR) + ' ' + rm.Unit AS QuantityText,
                    FORMAT(po.DueDate, 'MMM dd') AS DueDateText,
                    po.Status,
                    CASE 
                        WHEN po.Status = 'Ready' THEN '#DCFCE7'
                        WHEN po.Status = 'In Production' THEN '#DBEAFE'
                        ELSE '#F1F5F9'
                    END AS StatusBg,
                    CASE 
                        WHEN po.Status = 'Ready' THEN '#15803D'
                        WHEN po.Status = 'In Production' THEN '#1D4ED8'
                        ELSE '#475569'
                    END AS StatusColor
                FROM PurchaseOrders po
                JOIN RawMaterials rm ON po.MaterialID = rm.MaterialID
                WHERE po.SupplierID = @SupplierID
                ORDER BY po.DueDate DESC";

            return await ExecuteQueryAsync(query, new Dictionary<string, object> { { "@SupplierID", supplierId } });
        }
    }
}
