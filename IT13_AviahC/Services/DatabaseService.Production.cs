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
                INSERT INTO ProductionBatches (BatchID, ProductID, TargetQuantity, Status)
                VALUES (@BatchID, @ProductID, @TargetQty, 'In Progress')";
            
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

            // If completed, add to Boutique stock
            if (result > 0 && status == "Completed")
            {
                string updateStock = "UPDATE Products SET StockQuantity = StockQuantity + @Qty WHERE ProductID = (SELECT ProductID FROM ProductionBatches WHERE BatchID = @BatchID)";
                await ExecuteNonQueryAsync(updateStock, new Dictionary<string, object> { { "@Qty", newTotal }, { "@BatchID", batchId } });
            }

            return result;
        }
    }
}
