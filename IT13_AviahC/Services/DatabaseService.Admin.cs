using System.Data;

namespace IT13_AviahC.Services
{
    /// <summary>
    /// Admin dashboard read-only queries for Customers, Suppliers, Sales, Production, Promotions, Deliveries.
    /// Used by: AdminCustomersPage, AdminSuppliersPage, AdminSalesPage, AdminProductionPage, AdminPromotionsPage, AdminLogisticsPage
    /// </summary>
    public partial class DatabaseService
    {
        public DataTable GetAllCustomers()
        {
            return ExecuteQuery("SELECT Id, FirstName + ' ' + LastName AS FullName, Email, 'N/A' AS Phone, 'N/A' AS Address, 'Active' AS Status FROM Users WHERE Role = 'Customer' ORDER BY FullName");
        }

        public DataTable GetAllSuppliers()
        {
            return ExecuteQuery("SELECT Id, FirstName + ' ' + LastName AS CompanyName, Email, 'Supplier' AS Category, 'Active' AS Status FROM Users WHERE Role = 'Supplier' ORDER BY CompanyName");
        }

        public DataTable GetAllPromotions()
        {
            return ExecuteQuery("SELECT * FROM Promotions ORDER BY StartDate DESC");
        }

        public async Task<DataTable> GetPromotionsAsync()
        {
            // Join with Promotions table to get real promotion data
            string query = @"
                SELECT p.*, pr.PromoCode, pr.DiscountValue, pr.PromotionName, pr.EndDate
                FROM Products p 
                INNER JOIN Promotions pr ON p.PromoId = pr.PromoID 
                WHERE pr.Status = 'Active' 
                AND (pr.EndDate IS NULL OR pr.EndDate >= GETDATE())
                ORDER BY p.ProductName";
            return await ExecuteQueryAsync(query);
        }

        public DataTable GetAllDeliveries()
        {
            return ExecuteQuery("SELECT * FROM Deliveries ORDER BY DeliveryID DESC");
        }

        public DataTable GetAllSales()
        {
            return ExecuteQuery("SELECT OrderID, OrderRef, TotalAmount, Status, OrderDate AS SalesDate FROM Orders ORDER BY SalesDate DESC");
        }

        public DataTable GetAllProduction()
        {
            return ExecuteQuery("SELECT PONumber AS ProductionID, MaterialID AS ProductRef, Quantity, Status, DueDate AS StartDate FROM PurchaseOrders ORDER BY StartDate DESC");
        }
    }
}
