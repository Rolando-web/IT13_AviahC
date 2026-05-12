using System.Data;
using System.Collections.ObjectModel;
using IT13_AviahC.Services;
using IT13_AviahC.Models;

namespace IT13_AviahC.Views.DeliveryStaff;

public partial class DeliveryStaffDashboardPage : ContentPage
{
    private readonly DatabaseService _db;
    public ObservableCollection<DeliveryItem> AssignedDeliveries { get; } = new();

    public DeliveryStaffDashboardPage()
    {
        InitializeComponent();
        _db = new DatabaseService();
        AssignedDeliveriesCollection.ItemsSource = AssignedDeliveries;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            string driverEmail = UserSession.UserEmail ?? "delivery@aviah.com";
            DataTable dt = await _db.GetDeliveriesForDriverAsync(driverEmail);

            var list = new List<DeliveryItem>();
            int toDeliver = 0;
            int completed = 0;
            int pending = 0;
            int delayed = 0;

            foreach (DataRow row in dt.Rows)
            {
                var item = new DeliveryItem
                {
                    DeliveryID = row["DeliveryID"]?.ToString() ?? "N/A",
                    OrderRef = row["OrderRef"]?.ToString() ?? "N/A",
                    CustomerName = row["CustomerName"]?.ToString() ?? "Unknown",
                    Status = row["Status"]?.ToString() ?? "Pending",
                    Destination = row["Destination"]?.ToString() ?? "Not set",
                    ETA = row["ETA"]?.ToString() ?? "TBD",
                    DriverName = row["DriverName"]?.ToString() ?? "Unassigned"
                };

                list.Add(item);

                // Stats calculation
                string status = item.Status.ToLower();
                if (status.Contains("delivered") || status.Contains("arrived") || status == "completed")
                    completed++;
                else if (status.Contains("transit") || status.Contains("delivery") || status.Contains("picked up"))
                    toDeliver++;
                else if (status.Contains("pending") || status.Contains("placed") || status.Contains("ship"))
                    pending++;
                
                if (status.Contains("delayed") || status.Contains("failed"))
                    delayed++;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AssignedDeliveries.Clear();
                foreach (var item in list) AssignedDeliveries.Add(item);

                ToDeliverLabel.Text = toDeliver.ToString();
                CompletedLabel.Text = completed.ToString();
                PendingLabel.Text = pending.ToString();
                DelayedLabel.Text = delayed.ToString();
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dashboard Load Error: {ex.Message}");
        }
    }
}
