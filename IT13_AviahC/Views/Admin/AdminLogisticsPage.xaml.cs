using System.Data;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin
{
    public class DeliveryItem
    {
        public string DeliveryID       { get; set; } = string.Empty;
        public string Status           { get; set; } = string.Empty;
        public string StatusColorDark  { get; set; } = "#624890";
        public string StatusColorLight { get; set; } = "#F3F0FA";
        public string DetailsText      { get; set; } = string.Empty;
        public string ETA              { get; set; } = "TBD";
        public string DriverName       { get; set; } = "Unassigned";
        public string CurrentLocation  { get; set; } = "Unknown";
        public string Destination      { get; set; } = "Not set";
    }

    public partial class AdminLogisticsPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        private string _selectedDeliveryId  = string.Empty;
        private string _selectedOrderRef    = string.Empty;

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

        // ─── LOAD ─────────────────────────────────────────────────────────────
        private async void LoadDeliveries()
        {
            try
            {
                // Use new async method from DatabaseService.Deliveries.cs
                DataTable dt = await _dbService.GetAllDeliveriesAsync();

                var deliveries      = new List<DeliveryItem>();
                var pendingOptions  = new List<string>();

                foreach (DataRow row in dt.Rows)
                {
                    string status     = row["Status"]?.ToString()      ?? "Pending";
                    string deliveryId = row["DeliveryID"]?.ToString()  ?? "N/A";
                    string orderRef   = row["OrderRef"]?.ToString()     ?? "N/A";
                    string customer   = row["CustomerName"]?.ToString() ?? "Unknown";

                    string dark  = "#624890";
                    string light = "#F3F0FA";

                    switch (status)
                    {
                        case "Delivered":
                            dark  = "#15803D"; light = "#DCFCE7"; break;
                        case "In Transit":
                        case "Out for Delivery":
                            dark  = "#1D4ED8"; light = "#DBEAFE"; break;
                        case "Pending":
                            dark  = "#D97706"; light = "#FEF3C7";
                            pendingOptions.Add($"{deliveryId} — {orderRef}");
                            break;
                        case "Failed":
                            dark  = "#DC2626"; light = "#FEE2E2"; break;
                    }

                    deliveries.Add(new DeliveryItem
                    {
                        DeliveryID       = deliveryId,
                        Status           = status,
                        StatusColorDark  = dark,
                        StatusColorLight = light,
                        DetailsText      = $"Ref: {orderRef} • {customer}",
                        ETA              = row["ETA"]?.ToString()              ?? "TBD",
                        DriverName       = row["DriverName"]?.ToString()       ?? "Unassigned",
                        CurrentLocation  = row["CurrentLocation"]?.ToString()  ?? "Unknown",
                        Destination      = row["Destination"]?.ToString()      ?? "Not set"
                    });
                }

                // Load real drivers from DB
                DataTable driversDt = await _dbService.GetAllDriversAsync();
                var driverList = new List<string>();
                foreach (DataRow r in driversDt.Rows)
                    driverList.Add(r["FullName"]?.ToString() ?? "Unknown");

                if (driverList.Count == 0)
                    driverList = new List<string> { "No drivers registered" };

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DeliveriesCollection.ItemsSource = deliveries;
                    DeliveryPicker.ItemsSource       = pendingOptions;
                    DriverPicker.ItemsSource         = driverList;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logistics Load Error: {ex.Message}");
                await DisplayAlertAsync("Error", "Failed to load deliveries: " + ex.Message, "OK");
            }
        }

        // ─── OPEN UPDATE MODAL ────────────────────────────────────────────────
        private void OnUpdateStatusClicked(object? sender, EventArgs e)
        {
            var button   = sender as Button;
            var delivery = button?.CommandParameter as DeliveryItem;
            if (delivery == null) return;

            _selectedDeliveryId = delivery.DeliveryID;
            // Extract OrderRef from DetailsText "Ref: ORD-xxx • Customer"
            _selectedOrderRef = delivery.DetailsText.Contains("•")
                ? delivery.DetailsText.Split('•')[0].Replace("Ref:", "").Trim()
                : string.Empty;

            ModalTitle.Text        = $"Update: {_selectedDeliveryId}";
            DetailsEntry.Text      = delivery.CurrentLocation;
            ModalOverlay.IsVisible = true;
        }

        private void OnCloseModalClicked(object? sender, EventArgs e)
        {
            ModalOverlay.IsVisible  = false;
            _selectedDeliveryId     = string.Empty;
            _selectedOrderRef       = string.Empty;
        }

        // ─── CONFIRM UPDATE ───────────────────────────────────────────────────
        private async void OnConfirmUpdateClicked(object? sender, EventArgs e)
        {
            if (StatusPicker.SelectedItem == null || string.IsNullOrEmpty(_selectedDeliveryId))
            {
                await DisplayAlertAsync("Required", "Please select a status.", "OK");
                return;
            }

            string newStatus     = StatusPicker.SelectedItem.ToString()!;
            string location      = DetailsEntry.Text?.Trim() ?? string.Empty;
            string driverName    = DriverPicker.SelectedItem?.ToString() ?? "Unassigned";

            // Use the unified UpdateDeliveryAsync which also syncs Orders.Status
            int result = await _dbService.UpdateDeliveryAsync(
                _selectedDeliveryId, newStatus, location,
                destination: string.Empty,   // keep existing destination
                eta: string.Empty,           // keep existing ETA
                driverName: driverName);

            if (result > 0)
            {
                await DisplayAlertAsync("Updated",
                    $"{_selectedDeliveryId} → '{newStatus}'.\nCustomer tracking has been updated.", "OK");
            }
            else
            {
                await DisplayAlertAsync("Failed", "Could not update delivery status.", "OK");
            }

            ModalOverlay.IsVisible  = false;
            _selectedDeliveryId     = string.Empty;
            _selectedOrderRef       = string.Empty;
            LoadDeliveries();
        }
    }
}
