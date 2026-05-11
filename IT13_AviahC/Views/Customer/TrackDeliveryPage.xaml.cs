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
            // Start loading data after the page has appeared
            Task.Run(async () => await LoadTrackingDataSafe());
        }

        private async Task LoadTrackingDataSafe()
        {
            try
            {
                string userEmail = UserSession.UserEmail ?? "Customer@aviah.com";
                DataTable dt = await _databaseService.GetActiveOrdersAsync(userEmail);

                // Use the MainThread to update UI
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            DataRow order = dt.Rows[0];
                            
                            // 1. Basic Visibility
                            TrackingContent.IsVisible = true;
                            EmptyStateView.IsVisible = false;

                            // 2. Map Labels with exhaustive null checks
                            if (OrderRefLabel != null)
                                OrderRefLabel.Text = order.Table.Columns.Contains("OrderRef") ? $"#{order["OrderRef"]}" : "#N/A";
                            
                            if (OrderDateLabel != null)
                            {
                                if (order.Table.Columns.Contains("OrderDate") && order["OrderDate"] != DBNull.Value)
                                {
                                    try { OrderDateLabel.Text = Convert.ToDateTime(order["OrderDate"]).ToString("MMM dd, yyyy HH:mm"); }
                                    catch { OrderDateLabel.Text = "--"; }
                                }
                                else { OrderDateLabel.Text = "--"; }
                            }

                            if (CustomerNameLabel != null)
                                CustomerNameLabel.Text = UserSession.UserName ?? "Valued Customer";

                            if (OrderItemsLabel != null)
                                OrderItemsLabel.Text = order.Table.Columns.Contains("ItemSummary") ? order["ItemSummary"]?.ToString() ?? "Order Items" : "Order Items";
                            
                            if (OrderAmountLabel != null)
                            {
                                if (order.Table.Columns.Contains("TotalAmount") && order["TotalAmount"] != DBNull.Value)
                                {
                                    try { OrderAmountLabel.Text = $"₱{Convert.ToDecimal(order["TotalAmount"]):N2}"; }
                                    catch { OrderAmountLabel.Text = "₱0.00"; }
                                }
                                else { OrderAmountLabel.Text = "₱0.00"; }
                            }

                            // 3. Delivery Info
                            string driverName = "Awaiting Driver";
                            if (order.Table.Columns.Contains("DriverName") && order["DriverName"] != DBNull.Value)
                            {
                                driverName = order["DriverName"]?.ToString() ?? "Awaiting Driver";
                            }
                            if (DriverLabel != null) DriverLabel.Text = $"Driver: {driverName}";
                            
                            if (ETALabel != null)
                            {
                                if (order.Table.Columns.Contains("ETA") && order["ETA"] != DBNull.Value)
                                {
                                    ETALabel.Text = $"ETA: {order["ETA"]}";
                                }
                                else
                                {
                                    ETALabel.Text = "ETA: Pending";
                                }
                            }

                            if (CurrentLocationLabel != null)
                            {
                                if (order.Table.Columns.Contains("CurrentLocation") && order["CurrentLocation"] != DBNull.Value)
                                {
                                    string loc = order["CurrentLocation"]?.ToString() ?? string.Empty;
                                    CurrentLocationLabel.Text = string.IsNullOrEmpty(loc) ? "Processing at Facility..." : loc;
                                }
                                else
                                {
                                    CurrentLocationLabel.Text = "Preparing items for shipment...";
                                }
                            }

                            string status = "Pending";
                            if (order.Table.Columns.Contains("Status") && order["Status"] != DBNull.Value)
                            {
                                status = order["Status"]?.ToString() ?? "Pending";
                            }
                            UpdateStatusUI(status);
                        }
                        else
                        {
                            TrackingContent.IsVisible = false;
                            EmptyStateView.IsVisible = true;
                        }
                    }
                    catch (Exception innerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"UI Mapping Error: {innerEx.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tracking: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (TrackingContent != null) TrackingContent.IsVisible = false;
                    if (EmptyStateView != null) EmptyStateView.IsVisible = true;
                });
            }
        }

        private void UpdateStatusUI(string status)
        {
            try
            {
                // Safety check for UI elements
                if (Step1Border == null || CurrentStatusLabel == null || StatusDetailLabel == null) return;

                // Reset colors using standard MAUI Color.FromUint or from hex string
                var gray = Color.FromArgb("#F1F5F9");
                var green = Color.FromArgb("#10B981");
                var red = Color.FromArgb("#EF4444");

                Step1Border.BackgroundColor = gray;
                if (Step2Border != null) Step2Border.BackgroundColor = gray;
                if (Step3Border != null) Step3Border.BackgroundColor = gray;
                if (Step4Border != null) Step4Border.BackgroundColor = gray;
                if (Step5Border != null) Step5Border.BackgroundColor = gray;

                string statusLower = (status ?? "Pending").ToLower();

                if (statusLower.Contains("placed") || statusLower == "pending")
                {
                    Step1Border.BackgroundColor = green;
                    CurrentStatusLabel.Text = "Order Placed";
                    StatusDetailLabel.Text = "Your order has been received and is waiting for validation.";
                }
                else if (statusLower == "paid" || statusLower == "confirmed")
                {
                    Step1Border.BackgroundColor = green;
                    if (Step2Border != null) Step2Border.BackgroundColor = green;
                    CurrentStatusLabel.Text = "Order Paid & Confirmed";
                    StatusDetailLabel.Text = "Payment received. We are preparing your items for shipment.";
                }
                else if (statusLower == "shipped" || statusLower.Contains("transit"))
                {
                    Step1Border.BackgroundColor = green;
                    if (Step2Border != null) Step2Border.BackgroundColor = green;
                    if (Step3Border != null) Step3Border.BackgroundColor = green;
                    CurrentStatusLabel.Text = "In Transit";
                    StatusDetailLabel.Text = "Your parcel has been shipped and is on its way to you.";
                }
                else if (statusLower.Contains("delivery") || statusLower == "delivering")
                {
                    Step1Border.BackgroundColor = green;
                    if (Step2Border != null) Step2Border.BackgroundColor = green;
                    if (Step3Border != null) Step3Border.BackgroundColor = green;
                    if (Step4Border != null) Step4Border.BackgroundColor = red;
                    CurrentStatusLabel.Text = "Out for Delivery";
                    StatusDetailLabel.Text = "Your parcel is with our delivery partner and will arrive today.";
                }
                else if (statusLower == "arrived" || statusLower == "delivered" || statusLower == "completed")
                {
                    Step1Border.BackgroundColor = green;
                    if (Step2Border != null) Step2Border.BackgroundColor = green;
                    if (Step3Border != null) Step3Border.BackgroundColor = green;
                    if (Step4Border != null) Step4Border.BackgroundColor = green;
                    if (Step5Border != null) Step5Border.BackgroundColor = green;
                    CurrentStatusLabel.Text = "Arrived";
                    StatusDetailLabel.Text = "Your parcel has arrived at your location!";
                }
                else
                {
                    Step1Border.BackgroundColor = green;
                    CurrentStatusLabel.Text = status;
                    StatusDetailLabel.Text = "We are currently processing your order status.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateStatusUI Error: {ex.Message}");
            }
        }
    }
}
