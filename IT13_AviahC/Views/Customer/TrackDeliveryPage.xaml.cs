using System.Data;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Customer
{
    public partial class TrackDeliveryPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public TrackDeliveryPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadTrackingDataSafe();
        }

        private async Task LoadTrackingDataSafe()
        {
            try
            {
                string userEmail = UserSession.UserEmail ?? "Customer@aviah.com";
                DataTable dt = await _databaseService.GetActiveOrdersAsync(userEmail);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        DataRow order = dt.Rows[0];
                        
                        // Show tracking content, hide empty state
                        TrackingContent.IsVisible = true;
                        EmptyStateView.IsVisible = false;

                        // Update Labels
                        OrderRefLabel.Text = $"#{order["OrderRef"]}";
                        OrderDateLabel.Text = order["OrderDate"] != DBNull.Value ? Convert.ToDateTime(order["OrderDate"]).ToString("MM/dd/yyyy HH:mm") : "--";
                        CustomerNameLabel.Text = UserSession.UserName ?? "Valued Customer";
                        OrderItemsLabel.Text = order["ItemSummary"]?.ToString() ?? "Order Items";
                        OrderAmountLabel.Text = order["TotalAmount"] != DBNull.Value ? $"₱{Convert.ToDecimal(order["TotalAmount"]):N2}" : "₱0.00";

                        // Map Delivery Info
                        string driverName = order.Table.Columns.Contains("DriverName") && order["DriverName"] != DBNull.Value ? order["DriverName"].ToString() : "Awaiting Driver";
                        DriverLabel.Text = $"Driver: {driverName}";
                        
                        if (order.Table.Columns.Contains("ETA") && order["ETA"] != DBNull.Value)
                        {
                            string etaString = order["ETA"].ToString();
                            if (DateTime.TryParse(etaString, out DateTime etaDate))
                            {
                                ETALabel.Text = $"ETA: {etaDate.ToString("MMM dd, yyyy HH:mm")}";
                            }
                            else
                            {
                                ETALabel.Text = $"ETA: {etaString}";
                            }
                        }
                        else
                        {
                            ETALabel.Text = "ETA: Pending";
                        }

                        if (order.Table.Columns.Contains("CurrentLocation") && order["CurrentLocation"] != DBNull.Value)
                        {
                            string loc = order["CurrentLocation"].ToString();
                            CurrentLocationLabel.Text = string.IsNullOrEmpty(loc) ? "Processing at Facility..." : loc;
                        }
                        else
                        {
                            CurrentLocationLabel.Text = "Preparing items for shipment...";
                        }

                        string status = order["Status"]?.ToString() ?? "Pending";
                        UpdateStatusUI(status);
                    }
                    else
                    {
                        // Hide tracking content, show empty state
                        TrackingContent.IsVisible = false;
                        EmptyStateView.IsVisible = true;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tracking: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    TrackingContent.IsVisible = false;
                    EmptyStateView.IsVisible = true;
                });
            }
        }

        private void UpdateStatusUI(string status)
        {
            // Reset colors
            Step1Border.BackgroundColor = Color.Parse("#F1F5F9");
            Step2Border.BackgroundColor = Color.Parse("#F1F5F9");
            Step3Border.BackgroundColor = Color.Parse("#F1F5F9");
            Step4Border.BackgroundColor = Color.Parse("#F1F5F9");
            Step5Border.BackgroundColor = Color.Parse("#F1F5F9");

            // Simplified status mapping
            switch (status.ToLower())
            {
                case "pending":
                    Step1Border.BackgroundColor = Color.Parse("#10B981");
                    CurrentStatusLabel.Text = "Order Placed";
                    StatusDetailLabel.Text = "Your order has been received and is waiting for validation.";
                    break;
                case "paid":
                case "confirmed":
                    Step1Border.BackgroundColor = Color.Parse("#10B981");
                    Step2Border.BackgroundColor = Color.Parse("#10B981");
                    CurrentStatusLabel.Text = "Order Paid & Confirmed";
                    StatusDetailLabel.Text = "Payment received. We are preparing your items for shipment.";
                    break;
                case "shipped":
                case "in transit":
                    Step1Border.BackgroundColor = Color.Parse("#10B981");
                    Step2Border.BackgroundColor = Color.Parse("#10B981");
                    Step3Border.BackgroundColor = Color.Parse("#10B981");
                    CurrentStatusLabel.Text = "In Transit";
                    StatusDetailLabel.Text = "Your parcel has been shipped and is on its way to you.";
                    break;
                case "out for delivery":
                case "delivering":
                    Step1Border.BackgroundColor = Color.Parse("#10B981");
                    Step2Border.BackgroundColor = Color.Parse("#10B981");
                    Step3Border.BackgroundColor = Color.Parse("#10B981");
                    Step4Border.BackgroundColor = Color.Parse("#EF4444");
                    CurrentStatusLabel.Text = "Out for Delivery";
                    StatusDetailLabel.Text = "Your parcel is with our delivery partner and will arrive today.";
                    break;
                case "arrived":
                case "delivered":
                    Step1Border.BackgroundColor = Color.Parse("#10B981");
                    Step2Border.BackgroundColor = Color.Parse("#10B981");
                    Step3Border.BackgroundColor = Color.Parse("#10B981");
                    Step4Border.BackgroundColor = Color.Parse("#10B981");
                    Step5Border.BackgroundColor = Color.Parse("#10B981");
                    CurrentStatusLabel.Text = "Arrived";
                    StatusDetailLabel.Text = "Your parcel has arrived at your location!";
                    break;
            }
        }
    }
}
