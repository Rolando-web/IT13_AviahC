using System.Data;

namespace IT13_AviahC.Services
{
    public partial class DatabaseService
    {
        public async Task<DataTable> GetAllProductionBatchesAsync()
        {
            string query = @"
                SELECT pb.*, p.ProductName 
                FROM ProductionBatches pb
                JOIN Products p ON pb.ProductID = p.ProductID
                ORDER BY pb.StartDate DESC";
            return await ExecuteQueryAsync(query);
        }

        public async Task<int> CreateProductionBatchAsync(string batchId, int productId, int targetQty)
        {
            string query = @"
                INSERT INTO ProductionBatches (BatchID, ProductID, TargetQuantity, Status, StartDate)
                VALUES (@BatchID, @ProductID, @TargetQty, 'In Progress', GETDATE())";
            
            var parameters = new Dictionary<string, object>
            {
                { "@BatchID", batchId },
                { "@ProductID", productId },
                { "@TargetQty", targetQty }
            };
            return await ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<int> UpdateProductionOutputAsync(string batchId, int produced, int defects)
        {
            // First check if the update would complete the batch
            string checkQuery = "SELECT TargetQuantity, ProducedQuantity FROM ProductionBatches WHERE BatchID = @BatchID";
            var dt = await ExecuteQueryAsync(checkQuery, new Dictionary<string, object> { { "@BatchID", batchId } });
            
            if (dt.Rows.Count == 0) return 0;

            int target = Convert.ToInt32(dt.Rows[0]["TargetQuantity"]);
            int currentProduced = Convert.ToInt32(dt.Rows[0]["ProducedQuantity"]);
            int newTotal = currentProduced + produced;
            
            string status = newTotal >= target ? "Completed" : "In Progress";
            
            string query = @"
                UPDATE ProductionBatches 
                SET ProducedQuantity = ProducedQuantity + @Produced,
                    Defects = Defects + @Defects,
                    Status = @Status,
                    EndDate = CASE WHEN @Status = 'Completed' THEN GETDATE() ELSE EndDate END
                WHERE BatchID = @BatchID";

            var parameters = new Dictionary<string, object>
            {
                { "@BatchID", batchId },
                { "@Produced", produced },
                { "@Defects", defects },
                { "@Status", status }
            };
            
            int result = await ExecuteNonQueryAsync(query, parameters);

            // If completed, add to WAREHOUSE stock (Staff will then Stock-in to Boutique)
            if (result > 0 && status == "Completed")
            {
                string updateStock = "UPDATE Products SET WarehouseStock = ISNULL(WarehouseStock, 0) + @Qty WHERE ProductID = (SELECT ProductID FROM ProductionBatches WHERE BatchID = @BatchID)";
                await ExecuteNonQueryAsync(updateStock, new Dictionary<string, object> { { "@Qty", newTotal }, { "@BatchID", batchId } });
            }

            return result;
        }
        public async Task<DataTable> GetFinishedProductsAsync()
        {
            // Products with at least one recipe entry
            string query = @"
                SELECT DISTINCT p.* 
                FROM Products p
                JOIN ProductMaterials pm ON p.ProductID = pm.ProductID
                ORDER BY p.ProductName";
            return await ExecuteQueryAsync(query);
        }

        public async Task<DataTable> GetProductMaterialsAsync(int productId)
        {
            string query = @"
                SELECT pm.*, rm.ItemName as MaterialName, rm.Quantity as StockQuantity
                FROM ProductMaterials pm
                JOIN RawMaterials rm ON pm.MaterialID = rm.MaterialID
                WHERE pm.ProductID = @ProductID";
            var parameters = new Dictionary<string, object> { { "@ProductID", productId } };
            return await ExecuteQueryAsync(query, parameters);
        }
    }
}
