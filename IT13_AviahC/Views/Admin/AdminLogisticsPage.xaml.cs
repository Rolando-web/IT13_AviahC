using System.Data;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin
{
    public class DeliveryItem
    {
        public string DeliveryID { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColorDark { get; set; } = "#624890";
        public string StatusColorLight { get; set; } = "#F3F0FA";
        public string DetailsText { get; set; } = string.Empty;
        public string ETA { get; set; } = "TBD";
        public string DriverName { get; set; } = "Unassigned";
        public string CurrentLocation { get; set; } = "Unknown";
        public string Destination { get; set; } = "Not set";
    }

    public partial class AdminLogisticsPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public AdminLogisticsPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadDeliveries();
        }

        private async void LoadDeliveries()
        {
            try
            {
                // Run database query on a background thread to prevent UI freezing/crashing
                DataTable dt = await Task.Run(() => _dbService.GetAllDeliveries());
                
                var deliveries = new List<DeliveryItem>();
                var pendingDeliveries = new List<string>();

                foreach (DataRow row in dt.Rows)
                {
                    string status = row["Status"]?.ToString() ?? "Pending";
                    string deliveryId = row["DeliveryID"]?.ToString() ?? "N/A";
                    string orderRef = row["OrderRef"]?.ToString() ?? "N/A";
                    string customerName = row["CustomerName"]?.ToString() ?? "Unknown";

                    string statusColorDark = "#624890";
                    string statusColorLight = "#F3F0FA";

                    if (status == "Delivered")
                    {
                        statusColorDark = "#15803D";
                        statusColorLight = "#DCFCE7";
                    }
                    else if (status == "In Transit")
                    {
                        statusColorDark = "#1D4ED8";
                        statusColorLight = "#DBEAFE";
                    }
                    else if (status == "Pending")
                    {
                        statusColorDark = "#D97706";
                        statusColorLight = "#FFEDD5";
                        pendingDeliveries.Add($"{deliveryId} (Pending)");
                    }

                    deliveries.Add(new DeliveryItem
                    {
                        DeliveryID = deliveryId,
                        Status = status,
                        StatusColorDark = statusColorDark,
                        StatusColorLight = statusColorLight,
                        DetailsText = $"Ref: {orderRef} • {customerName}",
                        ETA = row["ETA"]?.ToString() ?? "TBD",
                        DriverName = row["DriverName"]?.ToString() ?? "Unassigned",
                        CurrentLocation = row["CurrentLocation"]?.ToString() ?? "Unknown",
                        Destination = row["Destination"]?.ToString() ?? "Not set"
                    });
                }

                // Update UI on the main thread
                MainThread.BeginInvokeOnMainThread(() => 
                {
                    DeliveriesCollection.ItemsSource = deliveries;
                    DeliveryPicker.ItemsSource = pendingDeliveries;
                    
                    // Sample drivers for the picker
                    DriverPicker.ItemsSource = new List<string> { "John Doe (Van 1)", "Jane Smith (Van 2)", "Mike Ross (Bike 1)" };
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logistics Load Error: {ex.Message}");
                await DisplayAlertAsync("Error", "Failed to load deliveries: " + ex.Message, "OK");
            }
        }

        private void OnUpdateStatusClicked(object? sender, EventArgs e)
        {
            var button = sender as Button;
            var delivery = button?.CommandParameter as DeliveryItem;
            if (delivery != null)
            {
                ModalTitle.Text = $"Update Order Status: {delivery.DeliveryID}";
                ModalOverlay.IsVisible = true;
            }
        }

        private void OnCloseModalClicked(object? sender, EventArgs e)
        {
            ModalOverlay.IsVisible = false;
        }

        private async void OnConfirmUpdateClicked(object? sender, EventArgs e)
        {
            if (StatusPicker.SelectedItem == null)
            {
                await DisplayAlertAsync("Selection Required", "Please select a status update.", "OK");
                return;
            }

            // Here you would normally update the database
            await DisplayAlertAsync("Success", "Delivery status has been updated successfully.", "OK");
            ModalOverlay.IsVisible = false;
            LoadDeliveries(); // Refresh list
        }
    }
}
