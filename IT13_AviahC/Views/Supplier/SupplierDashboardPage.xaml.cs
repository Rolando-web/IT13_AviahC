using System.Data;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Supplier;

public partial class SupplierDashboardPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public SupplierDashboardPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadDashboardData();
    }

    private async void LoadDashboardData()
    {
        try
        {
            // Use current session email (or hardcoded for now if login not fully session-mapped)
            string supplierEmail = "supplier@aviah.com"; 
            DataTable dt = await _dbService.GetSupplierDashboardStatsAsync(supplierEmail);

            if (dt.Rows.Count > 0)
            {
                // Update Card Stats (taken from first row)
                ActiveOrdersLabel.Text = dt.Rows[0]["ActiveOrders"].ToString();
                MaterialRequestsLabel.Text = dt.Rows[0]["MaterialRequests"].ToString();
                LeadTimeLabel.Text = "4 Days"; // Placeholder or can be calculated

                // Map rows to PO list
                var orders = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    if (row["PONumber"] != DBNull.Value)
                    {
                        orders.Add(new
                        {
                            PONumber = row["PONumber"].ToString(),
                            MaterialName = row["MaterialName"].ToString(),
                            QuantityText = row["QuantityText"].ToString(),
                            DueDateText = row["DueDateText"].ToString(),
                            Status = row["Status"].ToString(),
                            StatusBg = row["StatusBg"].ToString(),
                            StatusColor = row["StatusColor"].ToString()
                        });
                    }
                }

                RecentOrdersCollection.ItemsSource = orders;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to load dashboard: " + ex.Message, "OK");
        }
    }
}
