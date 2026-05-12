using System.Data;
using System.Collections.ObjectModel;
using IT13_AviahC.Services;
using IT13_AviahC.Models;

namespace IT13_AviahC.Views.DeliveryStaff;

public partial class DeliveryStaffLogisticsPage : ContentPage
{
    private readonly DatabaseService _db;
    private string _activeDeliveryId = string.Empty;
    public ObservableCollection<DeliveryItem> Deliveries { get; } = new();

    public DeliveryStaffLogisticsPage()
    {
        InitializeComponent();
        _db = new DatabaseService();
        DeliveriesCollection.ItemsSource = Deliveries;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadDeliveriesAsync();
    }

    private async Task LoadDeliveriesAsync()
    {
        try
        {
            string email = UserSession.UserEmail ?? string.Empty;
            DataTable dt = await _db.GetAllDeliveriesAsync(); // For logistics, show all or filtered

            var list = new List<DeliveryItem>();
            var pending = new List<DeliveryItem>();

            foreach (DataRow row in dt.Rows)
            {
                var item = new DeliveryItem
                {
                    DeliveryID = row["DeliveryID"]?.ToString() ?? "N/A",
                    OrderRef = row["OrderRef"]?.ToString() ?? "N/A",
                    CustomerName = row["CustomerName"]?.ToString() ?? "Unknown",
                    Status = row["Status"]?.ToString() ?? "Pending",
                    CurrentLocation = row["CurrentLocation"]?.ToString() ?? "Unknown",
                    Destination = row["Destination"]?.ToString() ?? "Not set",
                    ETA = row["ETA"]?.ToString() ?? "TBD",
                    DriverName = row["DriverName"]?.ToString() ?? "Unassigned",
                    ProductImage = row["ProductImage"]?.ToString() ?? "dress.png" // Default if null
                };

                list.Add(item);
                if (item.Status == "Pending" || item.Status == "Order placed")
                    pending.Add(item);
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Deliveries.Clear();
                // Filter out completed deliveries
                var activeItems = list.Where(i => !i.Status.Equals("Package delivered successfully", StringComparison.OrdinalIgnoreCase) 
                                               && !i.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)).ToList();
                
                foreach (var item in activeItems) Deliveries.Add(item);
                TotalLoadLabel.Text = activeItems.Count.ToString();
                PendingDeliveryPicker.ItemsSource = pending;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logistics Error: {ex.Message}");
        }
    }

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

    private void OnUpdateDeliveryClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is DeliveryItem item)
        {
            _activeDeliveryId = item.DeliveryID;
            UpdateModalTitle.Text = $"Update Order Status: {item.OrderRef}";
            ModalLocationEntry.Text = item.CurrentLocation;
            
            // Map "Pending" to the first step if necessary
            string currentStatus = item.Status;
            if (currentStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                currentStatus = _statusSequence[0];

            // Find current position in sequence
            int currentIndex = _statusSequence.FindIndex(s => s.Equals(currentStatus, StringComparison.OrdinalIgnoreCase));
            
            var availableStatuses = new List<string>();
            if (currentIndex == -1) 
            {
                // If unknown status, force them to start from the first step
                availableStatuses.Add(_statusSequence[0]);
            }
            else
            {
                // Always allow keeping the CURRENT status (useful for just updating location)
                availableStatuses.Add(_statusSequence[currentIndex]);
                
                // ONLY allow the IMMEDIATE next step. No skipping allowed.
                if (currentIndex + 1 < _statusSequence.Count)
                {
                    availableStatuses.Add(_statusSequence[currentIndex + 1]);
                }
            }

            ModalStatusPicker.ItemsSource = availableStatuses;
            ModalStatusPicker.SelectedIndex = 0; // Default to current status
            
            UpdateModalOverlay.IsVisible = true;
        }
    }

    private async void OnConfirmUpdateClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_activeDeliveryId)) return;

        string location = ModalLocationEntry.Text?.Trim() ?? string.Empty;
        
        // REQUIREMENT: Must provide location for parcel journey update
        if (string.IsNullOrWhiteSpace(location))
        {
            await DisplayAlert("Location Required", "Please enter the current location or status details (e.g., 'Sorting Facility', 'At Hub').", "OK");
            return;
        }

        string status = ModalStatusPicker.SelectedItem?.ToString() ?? "In Transit";

        int result = await _db.UpdateDeliveryAsync(_activeDeliveryId, status, location, "", "", "");
        if (result > 0)
        {
            UpdateModalOverlay.IsVisible = false;
            await LoadDeliveriesAsync();
        }
    }

    private async void OnViewDeliveredClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DeliveryStaffDeliveredPage());
    }

    private void OnCloseUpdateModalClicked(object sender, EventArgs e)
    {
        UpdateModalOverlay.IsVisible = false;
    }

    private async void OnAssignClicked(object sender, EventArgs e)
    {
        if (PendingDeliveryPicker.SelectedItem is DeliveryItem item)
        {
            await _db.UpdateDeliveryAsync(item.DeliveryID, "Preparing to ship", "Warehouse", "", "", "Assigned");
            await LoadDeliveriesAsync();
            await DisplayAlertAsync("Assigned", $"Delivery {item.DeliveryID} is now being prepared.", "OK");
        }
    }
}
