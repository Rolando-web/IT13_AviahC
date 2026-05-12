using System.Data;

namespace IT13_AviahC.Services
{
    /// <summary>
    /// Product/Inventory CRUD operations.
    /// Used by: StaffInventoryPage, BoutiquePage
    /// </summary>
    public partial class DatabaseService
    {
        public DataTable GetAllInventory()
        {
            // Join with Promotions if possible, otherwise return product data.
            // Using COALESCE/CASE to provide a virtual SKU and Status since they aren't in the DB table yet.
            string query = @"
                SELECT p.ProductID,
                       p.ProductName,
                       p.Category,
                       p.StockQuantity,
                       p.Price,
                       p.ImageUrl,
                       p.Description,
                       'PROD-' + CAST(p.ProductID AS VARCHAR) AS SKU,
                       CASE 
                           WHEN p.StockQuantity <= 0 THEN 'Out of Stock'
                           WHEN p.StockQuantity <= 10 THEN 'Low Stock'
                           ELSE 'In Stock'
                       END AS Status
                FROM Products p 
                ORDER BY p.ProductName";
            return ExecuteQuery(query);
        }

        public async Task<DataTable> GetAllInventoryAsync()
        {
            string query = @"
                SELECT p.ProductID,
                       p.ProductName,
                       p.Category,
                       p.StockQuantity,
                       p.Price,
                       p.DiscountPrice,
                       p.IsOnPromotion,
                       p.ImageUrl,
                       p.Description,
                       'PROD-' + CAST(p.ProductID AS VARCHAR) AS SKU,
                       CASE 
                           WHEN p.StockQuantity <= 0 THEN 'Out of Stock'
                           ELSE 'Available Stock: ' + CAST(p.StockQuantity AS VARCHAR)
                       END AS Status
                FROM Products p 
                ORDER BY p.ProductName";
            return await ExecuteQueryAsync(query);
        }

        public async Task<DataTable> GetBoutiqueProductsAsync()
        {
            string query = @"
                SELECT *, 
                       CASE 
                           WHEN StockQuantity <= 0 THEN 'Out of Stock'
                           ELSE 'Available Stock: ' + CAST(StockQuantity AS VARCHAR)
                       END AS Status
                FROM Products 
                WHERE IsOnPromotion = 0 OR IsOnPromotion IS NULL 
                ORDER BY ProductName";
            return await ExecuteQueryAsync(query);
        }

        public async Task<DataTable> GetPromotedProductsAsync()
        {
            string query = @"
                SELECT *, 
                       CASE 
                           WHEN StockQuantity <= 0 THEN 'Out of Stock'
                           ELSE 'Available Stock: ' + CAST(StockQuantity AS VARCHAR)
                       END AS Status
                FROM Products 
                WHERE IsOnPromotion = 1 
                ORDER BY ProductName";
            return await ExecuteQueryAsync(query);
        }

        public async Task<int> SetProductPromotionAsync(int productId, bool isOnPromotion, decimal? discountPrice)
        {
            string query = "UPDATE Products SET IsOnPromotion = @Promo, DiscountPrice = @Price WHERE ProductID = @Id";
            var parameters = new Dictionary<string, object>
            {
                { "@Id", productId },
                { "@Promo", isOnPromotion ? 1 : 0 },
                { "@Price", discountPrice ?? (object)DBNull.Value }
            };
            return await ExecuteNonQueryAsync(query, parameters);
        }

        public int AddInventory(string productName, string sku, string category, int stock, string unit, decimal price, string status, string imageUrl, int? promoId = null)
        {
            string query = "INSERT INTO Products (ProductName, Category, StockQuantity, Price, ImageUrl, Description, IsOnPromotion) VALUES (@ProductName, @Category, @Stock, @Price, @ImageUrl, @Description, 0)";
            var parameters = new Dictionary<string, object>
            {
                { "@ProductName", productName },
                { "@Category", category },
                { "@Stock", stock },
                { "@Price", price },
                { "@ImageUrl", imageUrl },
                { "@Description", string.Empty }
            };
            return ExecuteNonQuery(query, parameters);
        }

        public int UpdateInventory(int id, string productName, string sku, string category, int stock, string unit, decimal price, string status, string imageUrl, int? promoId = null)
        {
            string query = "UPDATE Products SET ProductName = @ProductName, Category = @Category, StockQuantity = @Stock, Price = @Price, ImageUrl = @ImageUrl WHERE ProductID = @Id";
            var parameters = new Dictionary<string, object>
            {
                { "@Id", id },
                { "@ProductName", productName },
                { "@Category", category },
                { "@Stock", stock },
                { "@Price", price },
                { "@ImageUrl", imageUrl }
            };
            return ExecuteNonQuery(query, parameters);
        }

        public int DeleteInventory(int id)
        {
            string query = "DELETE FROM Products WHERE ProductID = @Id";
            var parameters = new Dictionary<string, object> { { "@Id", id } };
            return ExecuteNonQuery(query, parameters);
        }

        public async Task<DataTable> GetAllWarehouseItemsAsync()
        {
            string query = @"
                SELECT MaterialID,
                       ItemName,
                       Category,
                       Quantity,
                       Unit,
                       Status,
                       ImageUrl
                FROM RawMaterials
                ORDER BY ItemName";
            return await ExecuteQueryAsync(query);
        }

        public int AddWarehouseItem(string materialId, string itemName, string category, int quantity, string unit, string status, string imageUrl)
        {
            string query = @"
                INSERT INTO RawMaterials (MaterialID, ItemName, Category, Quantity, Unit, Status, ImageUrl)
                VALUES (@MaterialID, @ItemName, @Category, @Quantity, @Unit, @Status, @ImageUrl)";
            var parameters = new Dictionary<string, object>
            {
                { "@MaterialID", materialId },
                { "@ItemName", itemName },
                { "@Category", category },
                { "@Quantity", quantity },
                { "@Unit", unit },
                { "@Status", status },
                { "@ImageUrl", imageUrl }
            };
            return ExecuteNonQuery(query, parameters);
        }

        public int UpdateWarehouseItem(string materialId, string newMaterialId, string itemName, string category, int quantity, string unit, string status, string imageUrl)
        {
            string query = @"
                UPDATE RawMaterials
                SET MaterialID = @NewMaterialID,
                    ItemName = @ItemName,
                    Category = @Category,
                    Quantity = @Quantity,
                    Unit = @Unit,
                    Status = @Status,
                    ImageUrl = @ImageUrl
                WHERE MaterialID = @MaterialID";
            var parameters = new Dictionary<string, object>
            {
                { "@MaterialID", materialId },
                { "@NewMaterialID", newMaterialId },
                { "@ItemName", itemName },
                { "@Category", category },
                { "@Quantity", quantity },
                { "@Unit", unit },
                { "@Status", status },
                { "@ImageUrl", imageUrl }
            };
            return ExecuteNonQuery(query, parameters);
        }

        public async Task<int> DeleteWarehouseItemAsync(string materialId)
        {
            // Remove FK-dependent PurchaseOrders first to avoid constraint violation
            await ExecuteNonQueryAsync(
                "DELETE FROM PurchaseOrders WHERE MaterialID = @MaterialID",
                new Dictionary<string, object> { { "@MaterialID", materialId } });

            return await ExecuteNonQueryAsync(
                "DELETE FROM RawMaterials WHERE MaterialID = @MaterialID",
                new Dictionary<string, object> { { "@MaterialID", materialId } });
        }
    }
}
