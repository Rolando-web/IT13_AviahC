using System.Data;
using IT13_AviahC.Services;
using IT13_AviahC.Models;

namespace IT13_AviahC.Views.Admin
{
    public partial class AdminLogisticsPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        private string _selectedDeliveryId = string.Empty;
        private readonly List<string> _statusSequence = new()
        {
            "Order placed",
            "Preparing to ship",
            "Your parcel has been picked up by our logistics partner",
            "Parcel has been received at dropoff point",
            "Parcel has arrived at sorting facility",
            "Parcel is loaded into truck, to leave sorting center soon",
            "Parcel has departed from sorting facility",
            "The order is in transit and is on its way to the next location",
            "Out for delivery",
            "Package delivered successfully"
        };

        public AdminLogisticsPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadDeliveries();
        }

        private async Task LoadDeliveries()
        {
            try
            {
                DataTable dt = await _dbService.GetAllDeliveriesAsync();
                var deliveries = new List<DeliveryItem>();
                var pendingDeliveries = new List<DeliveryItem>();

                foreach (DataRow row in dt.Rows)
                {
                    var item = new DeliveryItem
                    {
                        DeliveryID = row["DeliveryID"]?.ToString() ?? "N/A",
                        OrderRef = row["OrderRef"]?.ToString() ?? "N/A",
                        CustomerName = row["CustomerName"]?.ToString() ?? "Unknown",
                        Status = row["Status"]?.ToString() ?? "Pending",
                        ETA = row["ETA"]?.ToString() ?? "TBD",
                        DriverName = row["DriverName"]?.ToString() ?? "Unassigned",
                        CurrentLocation = row["CurrentLocation"]?.ToString() ?? "Unknown",
                        Destination = row["Destination"]?.ToString() ?? "Not set",
                        ProductImage = row["ProductImage"]?.ToString() ?? "dress.png"
                    };

                    deliveries.Add(item);

                    if (item.Status == "Pending" || item.Status == "Order placed")
                    {
                        pendingDeliveries.Add(item);
                    }
                }

                DataTable driversDt = await _dbService.GetAllDriversAsync();
                var driverList = new List<string>();
                foreach (DataRow r in driversDt.Rows)
                    driverList.Add(r["FullName"]?.ToString() ?? "Unknown");

                if (driverList.Count == 0)
                    driverList = new List<string> { "No drivers registered" };

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Filter out completed deliveries for the main list
                    var activeDeliveries = deliveries.Where(d => 
                        !d.Status.Equals("Package delivered successfully", StringComparison.OrdinalIgnoreCase) && 
                        !d.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)).ToList();

                    DeliveriesCollection.ItemsSource = activeDeliveries;
                    TotalLoadLabel.Text = activeDeliveries.Count.ToString();

                    DeliveryPicker.ItemsSource = pendingDeliveries;
                    DeliveryPicker.ItemDisplayBinding = new Binding("DeliveryID");
                    
                    DriverPicker.ItemsSource = driverList;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logistics Load Error: {ex.Message}");
            }
        }

        private void OnUpdateStatusClicked(object? sender, EventArgs e)
        {
            var button = sender as Button;
            var delivery = button?.CommandParameter as DeliveryItem;
            if (delivery == null) return;

            _selectedDeliveryId = delivery.DeliveryID;

            ModalTitle.Text = $"Update Status: {delivery.OrderRef}";
            DetailsEntry.Text = delivery.CurrentLocation;

            // Enforce sequence logic
            string currentStatus = delivery.Status;
            if (currentStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                currentStatus = _statusSequence[0];

            int currentIndex = _statusSequence.FindIndex(s => s.Equals(currentStatus, StringComparison.OrdinalIgnoreCase));
            var availableStatuses = new List<string>();

            if (currentIndex == -1)
            {
                availableStatuses.Add(_statusSequence[0]);
            }
            else
            {
                availableStatuses.Add(_statusSequence[currentIndex]);
                if (currentIndex + 1 < _statusSequence.Count)
                {
                    availableStatuses.Add(_statusSequence[currentIndex + 1]);
                }
            }

            StatusPicker.ItemsSource = availableStatuses;
            StatusPicker.SelectedIndex = 0;

            ModalOverlay.IsVisible = true;
        }

        private void OnCloseModalClicked(object? sender, EventArgs e)
        {
            ModalOverlay.IsVisible = false;
            _selectedDeliveryId = string.Empty;
        }

        private async void OnConfirmUpdateClicked(object? sender, EventArgs e)
        {
            if (StatusPicker.SelectedItem == null || string.IsNullOrEmpty(_selectedDeliveryId))
            {
                await DisplayAlertAsync("Required", "Please select a status.", "OK");
                return;
            }

            string location = DetailsEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(location))
            {
                await DisplayAlertAsync("Location Required", "Please enter current location details.", "OK");
                return;
            }

            string newStatus = StatusPicker.SelectedItem.ToString()!;

            int result = await _dbService.UpdateDeliveryAsync(
                _selectedDeliveryId, newStatus, location,
                destination: string.Empty,
                eta: string.Empty,
                driverName: string.Empty); // Don't change driver here

            if (result > 0)
            {
                ModalOverlay.IsVisible = false;
                _selectedDeliveryId = string.Empty;
                _ = LoadDeliveries();
            }
            else
            {
                await DisplayAlertAsync("Failed", "Could not update delivery status.", "OK");
            }
        }

        private async void OnViewHistoryClicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new AdminDeliveredPage());
        }

        private async void OnAssignClicked(object? sender, EventArgs e)
        {
            if (DeliveryPicker.SelectedItem is DeliveryItem delivery && DriverPicker.SelectedItem is string driverName)
            {
                if (driverName == "No drivers registered")
                {
                    await DisplayAlertAsync("Error", "No drivers available.", "OK");
                    return;
                }

                int result = await _dbService.UpdateDeliveryAsync(
                    delivery.DeliveryID, 
                    "Preparing to ship", 
                    "Warehouse", 
                    "", 
                    "", 
                    driverName);

                if (result > 0)
                {
                    await DisplayAlertAsync("Assigned", $"Delivery {delivery.DeliveryID} assigned to {driverName}.", "OK");
                    _ = LoadDeliveries();
                }
                else
                {
                    await DisplayAlertAsync("Failed", "Could not assign driver.", "OK");
                }
            }
            else
            {
                await DisplayAlertAsync("Required", "Please select both a delivery and a driver.", "OK");
            }
        }
    }
}
