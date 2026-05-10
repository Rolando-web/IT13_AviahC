using System.Data;

namespace IT13_AviahC.Services
{
    /// <summary>
    /// Order placement, tracking, and delivery record creation.
    /// Used by: BoutiquePage, MyOrdersPage, TrackDeliveryPage
    /// </summary>
    public partial class DatabaseService
    {
        public int PlaceOrder(string userEmail, string itemName, decimal amount)
        {
            string query = @"
                INSERT INTO Orders (OrderRef, UserId, OrderDate, TotalAmount, Status, ItemSummary)
                VALUES (@OrderRef, 
                        (SELECT TOP 1 Id FROM Users WHERE Email = @Email), 
                        GETDATE(), 
                        @Amount, 
                        'Pending', 
                        @ItemSummary)";
            
            string orderRef = "ORD-" + new Random().Next(10000, 99999);
            
            var parameters = new Dictionary<string, object>
            {
                { "@OrderRef", orderRef },
                { "@Email", userEmail },
                { "@Amount", amount },
                { "@ItemSummary", itemName }
            };
            
            int result = ExecuteNonQuery(query, parameters);
            
            if (result > 0)
            {
                // Create a Delivery record so it appears in the Logistics module
                string deliveryId = "DEL-" + new Random().Next(1000, 9999);
                string deliveryQuery = @"
                    INSERT INTO Deliveries (DeliveryID, OrderRef, CustomerName, Status, CreatedAt)
                    VALUES (@DeliveryID, @OrderRef, 
                            (SELECT TOP 1 FirstName + ' ' + LastName FROM Users WHERE Email = @Email), 
                            'Pending', 
                            GETDATE())";
                ExecuteNonQuery(deliveryQuery, new Dictionary<string, object> { 
                    { "@DeliveryID", deliveryId },
                    { "@OrderRef", orderRef },
                    { "@Email", userEmail }
                });
            }
            
            return result;
        }

        public async Task<int> PlaceOrderAsync(string userEmail, string itemName, decimal amount)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return PlaceOrder(userEmail, itemName, amount);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"PlaceOrderAsync Error: {ex.Message}");
                    return -1;
                }
            });
        }

        public DataTable GetAllOrders()
        {
            return ExecuteQuery("SELECT * FROM Orders ORDER BY OrderDate DESC");
        }

        public DataTable GetActiveOrders(string userEmail)
        {
            string query = @"
                SELECT * FROM Orders 
                WHERE UserId = (SELECT Id FROM Users WHERE Email = @Email)
                AND Status NOT IN ('Completed', 'Cancelled')
                ORDER BY OrderDate DESC";
            
            var parameters = new Dictionary<string, object> { { "@Email", userEmail } };
            return ExecuteQuery(query, parameters);
        }

        public async Task<DataTable> GetActiveOrdersAsync(string userEmail)
        {
            string query = @"
                SELECT o.*, d.ETA, d.DriverName, d.CurrentLocation, d.Destination 
                FROM Orders o 
                LEFT JOIN Deliveries d ON o.OrderRef = d.OrderRef 
                WHERE o.UserId = (SELECT Id FROM Users WHERE Email = @Email)
                AND o.Status NOT IN ('Completed', 'Cancelled')
                ORDER BY o.OrderDate DESC";
            return await ExecuteQueryAsync(query, new Dictionary<string, object> { { "@Email", userEmail } });
        }

        public async Task<DataTable> GetCustomerOrdersAsync(string userEmail)
        {
            string query = @"
                SELECT * FROM Orders 
                WHERE UserId = (SELECT Id FROM Users WHERE Email = @Email)
                ORDER BY OrderDate DESC";
            
            var parameters = new Dictionary<string, object> { { "@Email", userEmail } };
            return await ExecuteQueryAsync(query, parameters);
        }
    }
}
