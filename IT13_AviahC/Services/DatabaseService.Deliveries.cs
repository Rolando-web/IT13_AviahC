using System.Data;

namespace IT13_AviahC.Services
{
    /// <summary>
    /// Delivery Staff operations: load assigned deliveries, update location, status, ETA.
    /// Used by: DeliveryStaffLogisticsPage
    /// </summary>
    public partial class DatabaseService
    {
        // Load all deliveries (for Admin) or filtered by DriverID (for DeliveryStaff)
        public async Task<DataTable> GetDeliveriesForDriverAsync(string driverEmail)
        {
            string query = @"
                SELECT d.*, 
                       (SELECT TOP 1 p.ImageUrl 
                        FROM Products p 
                        INNER JOIN Orders o ON d.OrderRef = o.OrderRef 
                        WHERE o.ItemSummary LIKE '%' + p.ProductName + '%') AS ProductImage
                FROM Deliveries d
                WHERE d.DriverID = (SELECT TOP 1 Id FROM Users WHERE Email = @Email)
                   OR d.DriverID IS NULL
                ORDER BY d.CreatedAt DESC";
            return await ExecuteQueryAsync(query, new Dictionary<string, object> { { "@Email", driverEmail } });
        }

        public async Task<DataTable> GetAllDeliveriesAsync()
        {
            string query = @"
                SELECT d.*, 
                       (SELECT TOP 1 p.ImageUrl 
                        FROM Products p 
                        INNER JOIN Orders o ON d.OrderRef = o.OrderRef 
                        WHERE o.ItemSummary LIKE '%' + p.ProductName + '%') AS ProductImage
                FROM Deliveries d
                ORDER BY d.CreatedAt DESC";
            return await ExecuteQueryAsync(query);
        }

        public DataTable GetAllDeliveries()
        {
            string query = @"
                SELECT d.*, 
                       (SELECT TOP 1 p.ImageUrl 
                        FROM Products p 
                        INNER JOIN Orders o ON d.OrderRef = o.OrderRef 
                        WHERE o.ItemSummary LIKE '%' + p.ProductName + '%') AS ProductImage
                FROM Deliveries d
                ORDER BY d.CreatedAt DESC";
            return ExecuteQuery(query);
        }

        /// <summary>
        /// Called by Delivery Staff to update location, status, and ETA.
        /// Also syncs the Orders.Status to match.
        /// </summary>
        public async Task<int> UpdateDeliveryAsync(
            string deliveryId, string newStatus, string currentLocation,
            string destination, string eta, string driverName)
        {
            string updateDelivery = @"
                UPDATE Deliveries
                SET Status          = @Status,
                    CurrentLocation = CASE WHEN @CurrentLocation = '' THEN CurrentLocation ELSE @CurrentLocation END,
                    Destination     = CASE WHEN @Destination     = '' THEN Destination     ELSE @Destination     END,
                    ETA             = CASE WHEN @ETA             = '' THEN ETA             ELSE @ETA             END,
                    DriverName      = CASE WHEN @DriverName      = '' THEN DriverName      ELSE @DriverName      END
                WHERE DeliveryID = @DeliveryID";

            int result = await ExecuteNonQueryAsync(updateDelivery, new Dictionary<string, object>
            {
                { "@Status",          newStatus },
                { "@CurrentLocation", currentLocation },
                { "@Destination",     destination },
                { "@ETA",             eta },
                { "@DriverName",      driverName },
                { "@DeliveryID",      deliveryId }
            });

            if (result > 0)
            {
                string syncOrder = @"
                    UPDATE Orders SET Status = @Status
                    WHERE OrderRef = (SELECT TOP 1 OrderRef FROM Deliveries WHERE DeliveryID = @DeliveryID)";
                await ExecuteNonQueryAsync(syncOrder, new Dictionary<string, object>
                {
                    { "@Status",     newStatus },
                    { "@DeliveryID", deliveryId }
                });
            }

            return result;
        }

        /// <summary>Assign a driver (by userId) to a delivery.</summary>
        public async Task<int> AssignDriverAsync(string deliveryId, int driverUserId, string driverName)
        {
            string query = @"
                UPDATE Deliveries
                SET DriverID   = @DriverID,
                    DriverName = @DriverName,
                    Status     = CASE WHEN Status = 'Pending' THEN 'In Transit' ELSE Status END
                WHERE DeliveryID = @DeliveryID";

            return await ExecuteNonQueryAsync(query, new Dictionary<string, object>
            {
                { "@DriverID",    driverUserId },
                { "@DriverName",  driverName },
                { "@DeliveryID",  deliveryId }
            });
        }

        /// <summary>Get all Users with role DeliverStaff for driver picker.</summary>
        public async Task<DataTable> GetAllDriversAsync()
        {
            return await ExecuteQueryAsync(
                "SELECT Id, FirstName + ' ' + LastName AS FullName FROM Users WHERE Role = 'DeliverStaff' ORDER BY FullName");
        }
    }
}
