using System.Data;

namespace IT13_AviahC.Services
{
    public partial class DatabaseService
    {
        public async Task<Dictionary<string, object>> GetDashboardStatsAsync()
        {
            var stats = new Dictionary<string, object>();
            
            try
            {
                // 1. Monthly Sales
                DataTable dtMonthly = await ExecuteQueryAsync("SELECT ISNULL(SUM(TotalAmount), 0) as MonthlySales FROM Orders WHERE MONTH(OrderDate) = MONTH(GETDATE()) AND YEAR(OrderDate) = YEAR(GETDATE())");
                stats["MonthlySales"] = dtMonthly.Rows.Count > 0 ? Convert.ToDecimal(dtMonthly.Rows[0]["MonthlySales"]) : 0m;

                // 2. Total Orders
                DataTable dtOrders = await ExecuteQueryAsync("SELECT COUNT(*) as TotalOrders FROM Orders");
                stats["TotalOrders"] = dtOrders.Rows.Count > 0 ? Convert.ToInt32(dtOrders.Rows[0]["TotalOrders"]) : 0;

                // 3. Low Stock Items
                DataTable dtLowStock = await ExecuteQueryAsync("SELECT COUNT(*) as LowStock FROM Products WHERE StockQuantity < 20");
                stats["LowStock"] = dtLowStock.Rows.Count > 0 ? Convert.ToInt32(dtLowStock.Rows[0]["LowStock"]) : 0;

                // 4. Today's Sales
                DataTable dtToday = await ExecuteQueryAsync("SELECT ISNULL(SUM(TotalAmount), 0) as TodaySales FROM Orders WHERE CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)");
                stats["TodaySales"] = dtToday.Rows.Count > 0 ? Convert.ToDecimal(dtToday.Rows[0]["TodaySales"]) : 0m;

                // 5. Weekly Revenue
                DataTable dtWeekly = await ExecuteQueryAsync("SELECT ISNULL(SUM(TotalAmount), 0) as WeeklyRevenue FROM Orders WHERE OrderDate >= DATEADD(day, -7, GETDATE())");
                stats["WeeklyRevenue"] = dtWeekly.Rows.Count > 0 ? Convert.ToDecimal(dtWeekly.Rows[0]["WeeklyRevenue"]) : 0m;

                // 6. Pending Orders
                DataTable dtPending = await ExecuteQueryAsync("SELECT COUNT(*) as PendingOrders FROM Orders WHERE Status = 'Pending'");
                stats["PendingOrders"] = dtPending.Rows.Count > 0 ? Convert.ToInt32(dtPending.Rows[0]["PendingOrders"]) : 0;
                
                // 7. Total Customers
                DataTable dtCust = await ExecuteQueryAsync("SELECT COUNT(*) as NewCustomers FROM Users WHERE Role = 'Customer' AND CreatedAt >= DATEADD(month, -1, GETDATE())");
                stats["NewCustomers"] = dtCust.Rows.Count > 0 ? Convert.ToInt32(dtCust.Rows[0]["NewCustomers"]) : 0;

                // 8. Total Revenue (All time)
                DataTable dtTotalRev = await ExecuteQueryAsync("SELECT ISNULL(SUM(TotalAmount), 0) as TotalRevenue FROM Orders");
                stats["TotalRevenue"] = dtTotalRev.Rows.Count > 0 ? Convert.ToDecimal(dtTotalRev.Rows[0]["TotalRevenue"]) : 0m;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching dashboard stats: {ex.Message}");
            }

            return stats;
        }

        public async Task<DataTable> GetSalesOverviewAsync()
        {
            string query = @"
                SELECT 
                    FORMAT(OrderDate, 'MMM') as MonthName,
                    MONTH(OrderDate) as MonthNum,
                    SUM(TotalAmount) as Total
                FROM Orders 
                WHERE OrderDate >= DATEADD(month, -6, GETDATE())
                GROUP BY FORMAT(OrderDate, 'MMM'), MONTH(OrderDate)
                ORDER BY MonthNum";
            return await ExecuteQueryAsync(query);
        }

        public async Task<DataTable> GetPopularProductsAsync()
        {
            // heuristic: products mentioned most in ItemSummary
            string query = @"
                SELECT TOP 4 p.ProductName, p.Price, p.StockQuantity, p.ImageUrl,
                (SELECT COUNT(*) FROM Orders o WHERE o.ItemSummary LIKE '%' + p.ProductName + '%') as SalesCount
                FROM Products p
                ORDER BY SalesCount DESC";
            return await ExecuteQueryAsync(query);
        }

        public async Task<DataTable> GetSalesByCategoryAsync()
        {
            string query = @"
                SELECT Category, COUNT(*) as Count, SUM(Price) as TotalValue 
                FROM Products 
                GROUP BY Category";
            return await ExecuteQueryAsync(query);
        }
    }
}
