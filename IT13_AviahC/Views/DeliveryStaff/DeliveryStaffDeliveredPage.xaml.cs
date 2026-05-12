using System.Data;
using System.Collections.ObjectModel;
using IT13_AviahC.Services;
using IT13_AviahC.Models;

namespace IT13_AviahC.Views.DeliveryStaff;

public partial class DeliveryStaffDeliveredPage : ContentPage
{
    private readonly DatabaseService _db;
    public ObservableCollection<DeliveryItem> DeliveredOrders { get; } = new();

    public DeliveryStaffDeliveredPage()
    {
        InitializeComponent();
        _db = new DatabaseService();
        DeliveredCollection.ItemsSource = DeliveredOrders;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadDeliveredOrdersAsync();
    }

    private async Task LoadDeliveredOrdersAsync()
    {
        try
        {
            DataTable dt = await _db.GetAllDeliveriesAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                DeliveredOrders.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    string status = row["Status"]?.ToString() ?? "";
                    if (status.Equals("Package delivered successfully", StringComparison.OrdinalIgnoreCase) || 
                        status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        DeliveredOrders.Add(new DeliveryItem
                        {
                            DeliveryID = row["DeliveryID"]?.ToString() ?? "N/A",
                            OrderRef = row["OrderRef"]?.ToString() ?? "N/A",
                            CustomerName = row["CustomerName"]?.ToString() ?? "Unknown",
                            Status = status,
                            ProductImage = row["ProductImage"]?.ToString() ?? "dress.png"
                        });
                    }
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Delivered Orders Error: {ex.Message}");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
