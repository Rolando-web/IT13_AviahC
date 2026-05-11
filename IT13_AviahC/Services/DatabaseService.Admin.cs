using System.Data;

namespace IT13_AviahC.Services
{
    /// <summary>
    /// Admin dashboard read-only queries for Customers, Suppliers, Sales, Production, Promotions.
    /// Used by: AdminCustomersPage, AdminSuppliersPage, AdminSalesPage, AdminProductionPage, AdminPromotionsPage
    /// </summary>
    public partial class DatabaseService
    {
        public DataTable GetAllCustomers()
        {
            return ExecuteQuery(@"
                SELECT 
                    Id, 
                    FirstName + ' ' + LastName AS FullName, 
                    Email, 
                    'N/A' AS Phone, 
                    'N/A' AS Address, 
                    'Active' AS Status 
                FROM Users 
                WHERE Role = 'Customer' 
                ORDER BY FullName");
        }

        public DataTable GetAllSuppliers()
        {
            // Suppliers page expects: CompanyName, ContactPerson, Email, Category, Status
            return ExecuteQuery(@"
                SELECT 
                    Id, 
                    FirstName + ' ' + LastName AS CompanyName, 
                    FirstName AS ContactPerson,
                    Email, 
                    'Supplier' AS Category, 
                    'Active' AS Status 
                FROM Users 
                WHERE Role = 'Supplier' 
                ORDER BY CompanyName");
        }

        public DataTable GetAllPromotions()
        {
            return ExecuteQuery("SELECT * FROM Promotions ORDER BY StartDate DESC");
        }

        public async Task<DataTable> GetPromotionsAsync()
        {
            string query = @"
                SELECT p.*, pr.PromoCode, pr.DiscountValue, pr.PromotionName, pr.EndDate
                FROM Products p 
                INNER JOIN Promotions pr ON p.PromoId = pr.PromoID 
                WHERE pr.Status = 'Active' 
                AND (pr.EndDate IS NULL OR pr.EndDate >= GETDATE())
                ORDER BY p.ProductName";
            return await ExecuteQueryAsync(query);
        }

        public DataTable GetAllSales()
        {
            // Sales page expects: OrderID, CustomerName, ItemsSummary, SalesDate, TotalAmount, Status
            return ExecuteQuery(@"
                SELECT 
                    o.OrderRef AS OrderID, 
                    u.FirstName + ' ' + u.LastName AS CustomerName,
                    o.ItemSummary AS ItemsSummary,
                    o.OrderDate AS SalesDate,
                    o.TotalAmount,
                    o.Status
                FROM Orders o
                LEFT JOIN Users u ON o.UserId = u.Id
                ORDER BY o.OrderDate DESC");
        }

        public DataTable GetAllProduction()
        {
            // Production page expects: BatchID, ProductName, Status, Progress, EndDate, StartDate, TargetQuantity
            return ExecuteQuery(@"
                SELECT 
                    po.PONumber AS BatchID, 
                    rm.ItemName AS ProductName, 
                    po.Status, 
                    CASE 
                        WHEN po.Status = 'Ready' THEN 100 
                        WHEN po.Status = 'In Production' THEN 65 
                        ELSE 20 
                    END AS Progress,
                    po.DueDate AS EndDate,
                    DATEADD(day, -7, po.DueDate) AS StartDate,
                    po.Quantity AS TargetQuantity
                FROM PurchaseOrders po
                LEFT JOIN RawMaterials rm ON po.MaterialID = rm.MaterialID
                ORDER BY po.DueDate DESC");
        }
    }
}
