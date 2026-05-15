using System.Data;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin
{
    public partial class AdminSalesPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public AdminSalesPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadSales();
        }

        private async void LoadSales()
        {
            try
            {
                // Fetch Stats
                var stats = await _dbService.GetDashboardStatsAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    TodaySalesLabel.Text = string.Format("₱{0:N2}", stats["TodaySales"]);
                    WeeklyRevenueLabel.Text = string.Format("₱{0:N2}", stats["WeeklyRevenue"]);
                    PendingOrdersLabel.Text = stats["PendingOrders"].ToString();
                });

                // Fetch Transactions
                DataTable dt = _dbService.GetAllSales();
                var sales = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    string status = row["Status"]?.ToString() ?? string.Empty;
                    string statusColorDark = "#624890";
                    string statusColorLight = "#F3F0FA";

                    if (status.Contains("Delivered") || status == "Completed")
                    {
                        statusColorDark = "#15803D";
                        statusColorLight = "#DCFCE7";
                    }
                    else if (status == "Pending" || status == "Processing")
                    {
                        statusColorDark = "#92400E";
                        statusColorLight = "#FEF3C7";
                    }
                    else if (status.Contains("Transit"))
                    {
                        statusColorDark = "#1D4ED8";
                        statusColorLight = "#DBEAFE";
                    }

                    sales.Add(new
                    {
                        OrderID = row["OrderID"]?.ToString() ?? string.Empty,
                        CustomerName = row["CustomerName"]?.ToString() ?? string.Empty,
                        ItemsSummary = row["ItemSummary"]?.ToString() ?? row["ItemsSummary"]?.ToString() ?? string.Empty,
                        DateTime = row["OrderDate"]?.ToString() ?? row["SalesDate"]?.ToString() ?? string.Empty,
                        Amount = Convert.ToDecimal(row["TotalAmount"] ?? 0),
                        Status = status,
                        StatusColorDark = statusColorDark,
                        StatusColorLight = statusColorLight
                    });
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SalesCollection.ItemsSource = sales;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sales load error: {ex.Message}");
            }
        }
    }
}
