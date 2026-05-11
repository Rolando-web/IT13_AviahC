using System.Data;
using System.Collections.ObjectModel;
using IT13_AviahC.Services;
using IT13_AviahC.Models;

namespace IT13_AviahC.Views.DeliveryStaff;

public class DeliveryItem
{
    public string DeliveryID { get; set; } = string.Empty;
    public string OrderRef { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string CurrentLocation { get; set; } = "Unknown";
    public string Destination { get; set; } = "Not set";
    public string ETA { get; set; } = "TBD";
    public string DriverName { get; set; } = "Unassigned";
    public string ProductImage { get; set; } = "dress.png";

    public string StatusBgLight => Status switch
    {
        "Delivered" or "Package delivered successfully" => "#DCFCE7",
        "In Transit" or "Out for delivery" or "The order is in transit and is on its way to the next location" => "#EEF2FF",
        "Out for Delivery" => "#FEF3C7",
        "Failed" => "#FEE2E2",
        "Pending" or "Order placed" => "#F3F0FA",
        _ => "#F1F5F9"
    };

    public string StatusColorDark => Status switch
    {
        "Delivered" or "Package delivered successfully" => "#15803D",
        "In Transit" or "Out for delivery" or "The order is in transit and is on its way to the next location" => "#4338CA",
        "Out for Delivery" => "#D97706",
        "Failed" => "#DC2626",
        "Pending" or "Order placed" => "#624890",
        _ => "#64748B"
    };
}

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
                foreach (var item in list) Deliveries.Add(item);
                TotalLoadLabel.Text = list.Count.ToString();
                PendingDeliveryPicker.ItemsSource = pending;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logistics Error: {ex.Message}");
        }
    }

    private void OnUpdateDeliveryClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is DeliveryItem item)
        {
            _activeDeliveryId = item.DeliveryID;
            UpdateModalTitle.Text = $"Update Order Status: {item.OrderRef}";
            ModalLocationEntry.Text = item.CurrentLocation;
            UpdateModalOverlay.IsVisible = true;
        }
    }

    private async void OnConfirmUpdateClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_activeDeliveryId)) return;

        string status = ModalStatusPicker.SelectedItem?.ToString() ?? "In Transit";
        string location = ModalLocationEntry.Text ?? "Warehouse A";

        int result = await _db.UpdateDeliveryAsync(_activeDeliveryId, status, location, "", "", "");
        if (result > 0)
        {
            UpdateModalOverlay.IsVisible = false;
            await LoadDeliveriesAsync();
        }
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
            await DisplayAlert("Assigned", $"Delivery {item.DeliveryID} is now being prepared.", "OK");
        }
    }
}
