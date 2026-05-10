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
            string query = @"
                SELECT p.*, pr.PromoCode, pr.DiscountValue, pr.PromotionName 
                FROM Products p 
                LEFT JOIN Promotions pr ON p.PromoId = pr.PromoID 
                ORDER BY p.ProductName";
            return ExecuteQuery(query);
        }

        public async Task<DataTable> GetAllInventoryAsync()
        {
            string query = @"
                SELECT p.*, pr.PromoCode, pr.DiscountValue, pr.PromotionName 
                FROM Products p 
                LEFT JOIN Promotions pr ON p.PromoId = pr.PromoID 
                ORDER BY p.ProductName";
            return await ExecuteQueryAsync(query);
        }

        public int AddInventory(string productName, string sku, string category, int stock, string unit, decimal price, string status, string imageUrl, int? promoId = null)
        {
            string query = "INSERT INTO Products (ProductName, SKU, Category, StockLevel, Unit, Price, Status, ImageUrl, PromoId) VALUES (@ProductName, @SKU, @Category, @Stock, @Unit, @Price, @Status, @ImageUrl, @PromoId)";
            var parameters = new Dictionary<string, object>
            {
                { "@ProductName", productName },
                { "@SKU", sku },
                { "@Category", category },
                { "@Stock", stock },
                { "@Unit", unit },
                { "@Price", price },
                { "@Status", status },
                { "@ImageUrl", imageUrl },
                { "@PromoId", (object?)promoId ?? DBNull.Value }
            };
            return ExecuteNonQuery(query, parameters);
        }

        public int UpdateInventory(int id, string productName, string sku, string category, int stock, string unit, decimal price, string status, string imageUrl, int? promoId = null)
        {
            string query = "UPDATE Products SET ProductName = @ProductName, SKU = @SKU, Category = @Category, StockLevel = @Stock, Unit = @Unit, Price = @Price, Status = @Status, ImageUrl = @ImageUrl, PromoId = @PromoId WHERE Id = @Id";
            var parameters = new Dictionary<string, object>
            {
                { "@Id", id },
                { "@ProductName", productName },
                { "@SKU", sku },
                { "@Category", category },
                { "@Stock", stock },
                { "@Unit", unit },
                { "@Price", price },
                { "@Status", status },
                { "@ImageUrl", imageUrl },
                { "@PromoId", (object?)promoId ?? DBNull.Value }
            };
            return ExecuteNonQuery(query, parameters);
        }

        public int DeleteInventory(int id)
        {
            string query = "DELETE FROM Products WHERE Id = @Id";
            var parameters = new Dictionary<string, object> { { "@Id", id } };
            return ExecuteNonQuery(query, parameters);
        }
    }
}
