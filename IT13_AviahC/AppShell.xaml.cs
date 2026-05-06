namespace IT13_AviahC
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Admin Module
            Routing.RegisterRoute("AdminDashboard", typeof(IT13_AviahC.Views.Admin.AdminDashboardPage));
            Routing.RegisterRoute("AdminInventory", typeof(IT13_AviahC.Views.Admin.AdminInventoryPage));
            Routing.RegisterRoute("AdminProduction", typeof(IT13_AviahC.Views.Admin.AdminProductionPage));
            Routing.RegisterRoute("AdminSales", typeof(IT13_AviahC.Views.Admin.AdminSalesPage));
            Routing.RegisterRoute("AdminSuppliers", typeof(IT13_AviahC.Views.Admin.AdminSuppliersPage));
            Routing.RegisterRoute("AdminCustomers", typeof(IT13_AviahC.Views.Admin.AdminCustomersPage));
            Routing.RegisterRoute("AdminLogistics", typeof(IT13_AviahC.Views.Admin.AdminLogisticsPage));
            Routing.RegisterRoute("AdminReports", typeof(IT13_AviahC.Views.Admin.AdminReportsPage));
            Routing.RegisterRoute("AdminPromotions", typeof(IT13_AviahC.Views.Admin.AdminPromotionsPage));
            Routing.RegisterRoute("AdminSubscription", typeof(IT13_AviahC.Views.Admin.AdminSubscriptionPage));
            
            // Direct Global Routes for Middleware/Immediate Navigation
            Routing.RegisterRoute("DirectBoutique", typeof(IT13_AviahC.Views.Customer.BoutiquePage));
            Routing.RegisterRoute("DirectOffers", typeof(IT13_AviahC.Views.Customer.SpecialOffersPage));
            Routing.RegisterRoute("DirectOrders", typeof(IT13_AviahC.Views.Customer.MyOrdersPage));

            // Customer Shell Routes
            Routing.RegisterRoute("CustomerPortal", typeof(IT13_AviahC.Views.Customer.BoutiquePage));
            Routing.RegisterRoute("CustomerBoutique", typeof(IT13_AviahC.Views.Customer.BoutiquePage));
            Routing.RegisterRoute("CustomerOffers", typeof(IT13_AviahC.Views.Customer.SpecialOffersPage));
            Routing.RegisterRoute("CustomerOrders", typeof(IT13_AviahC.Views.Customer.MyOrdersPage));
        }
    }
}
