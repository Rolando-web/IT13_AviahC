using System.Data;
using IT13_AviahC.Services;
using IT13_AviahC.Models;

namespace IT13_AviahC.Views.Admin
{
    public partial class AdminDeliveredPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public AdminDeliveredPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadDeliveredOrders();
        }

        private async Task LoadDeliveredOrders()
        {
            try
            {
                DataTable dt = await _dbService.GetAllDeliveriesAsync();
                var list = new List<DeliveryItem>();

                foreach (DataRow row in dt.Rows)
                {
                    string status = row["Status"]?.ToString() ?? "";
                    if (status.Equals("Package delivered successfully", StringComparison.OrdinalIgnoreCase) || 
                        status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(new DeliveryItem
                        {
                            DeliveryID = row["DeliveryID"]?.ToString() ?? "N/A",
                            OrderRef = row["OrderRef"]?.ToString() ?? "N/A",
                            CustomerName = row["CustomerName"]?.ToString() ?? "Unknown",
                            Status = status,
                            DriverName = row["DriverName"]?.ToString() ?? "Unassigned",
                            ProductImage = row["ProductImage"]?.ToString() ?? "dress.png"
                        });
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DeliveredCollection.ItemsSource = list;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"History Error: {ex.Message}");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
