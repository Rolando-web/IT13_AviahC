using System.Data;
using IT13_AviahC.Models;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Customer
{
    public partial class MyOrdersPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public MyOrdersPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadOrdersSafe();
        }

        private async Task LoadOrdersSafe()
        {
            try
            {
                string userEmail = UserSession.UserEmail ?? "Customer@aviah.com";
                DataTable dt = await _databaseService.GetCustomerOrdersAsync(userEmail);
                var orders = new List<OrderItem>();

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string status = row["Status"]?.ToString() ?? "Pending";
                        DateTime orderDate = row["OrderDate"] != DBNull.Value ? Convert.ToDateTime(row["OrderDate"]) : DateTime.Now;
                        decimal amount = row["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalAmount"]) : 0m;

                        orders.Add(new OrderItem
                        {
                            OrderRef = row["OrderRef"]?.ToString() ?? "N/A",
                            OrderDateAndSummary = $"{orderDate:MMM dd, yyyy} • {row["ItemSummary"]}",
                            FormattedTotal = $"₱{amount:N2}",
                            Status = status,
                            StatusColor = GetStatusColor(status),
                            IsTrackable = status.ToLower() != "completed" && status.ToLower() != "cancelled"
                        });
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (orders.Count > 0)
                    {
                        OrdersCollection.IsVisible = true;
                        EmptyOrdersView.IsVisible = false;
                    }
                    else
                    {
                        OrdersCollection.IsVisible = false;
                        EmptyOrdersView.IsVisible = true;
                    }

                    OrdersCollection.ItemsSource = orders;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading orders: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OrdersCollection.IsVisible = false;
                    EmptyOrdersView.IsVisible = true;
                });
            }
        }

        private string GetStatusColor(string status)
        {
            return status.ToLower() switch
            {
                "pending" => "#805AD5",
                "paid" => "#3182CE",
                "shipped" => "#3182CE",
                "delivering" => "#DD6B20",
                "completed" => "#38A169",
                "cancelled" => "#E53E3E",
                _ => "#718096"
            };
        }

        private async void OnTrackClicked(object sender, EventArgs e)
        {
            try
            {
                // Use absolute path for more reliable navigation in complex Shell structures
                await Shell.Current.GoToAsync("//CustomerPortal/CustomerTrack");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Track Order Navigation Error: {ex.Message}");
                // Fallback to relative if absolute fails
                try { await Shell.Current.GoToAsync("///CustomerTrack"); } catch { }
            }
        }

        private async void OnShopClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("///CustomerBoutique");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shop Navigation Error: {ex.Message}");
            }
        }
    }
}
