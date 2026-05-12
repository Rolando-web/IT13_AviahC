using System.Data;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Customer
{
    public class JourneyLog
    {
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string FormattedTime { get; set; } = string.Empty;
        public bool IsLatest { get; set; } = false;
    }

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
            
            // Force refresh data every time page appears
            MainThread.BeginInvokeOnMainThread(async () => {
                await Task.Delay(300); 
                await LoadTrackingDataSafe();
            });
        }

        private async Task LoadTrackingDataSafe()
        {
            try
            {
                string userEmail = UserSession.UserEmail ?? "Customer@aviah.com";
                DataTable dt;

                if (!string.IsNullOrEmpty(UserSession.SelectedOrderRef))
                {
                    // Track SPECIFIC order
                    dt = await _databaseService.GetSpecificOrderTrackingAsync(UserSession.SelectedOrderRef);
                }
                else
                {
                    // Fallback to LATEST active order
                    dt = await _databaseService.GetActiveOrdersAsync(userEmail);
                }

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

                            string status = "Pending";
                            if (order.Table.Columns.Contains("Status") && order["Status"] != DBNull.Value)
                            {
                                status = order["Status"]?.ToString() ?? "Pending";
                            }
                            UpdateStatusUI(status);

                            // 4. LOAD PARCEL JOURNEY (HISTORY)
                            _ = LoadJourneyHistoryAsync(order["DeliveryID"]?.ToString() ?? string.Empty);
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

        private async Task LoadJourneyHistoryAsync(string deliveryId)
        {
            if (string.IsNullOrEmpty(deliveryId)) return;

            try
            {
                DataTable historyDt = await _databaseService.GetDeliveryHistoryAsync(deliveryId);
                var logs = new List<JourneyLog>();

                int count = 0;
                foreach (DataRow row in historyDt.Rows)
                {
                    DateTime updateTime = row["UpdateTime"] != DBNull.Value ? Convert.ToDateTime(row["UpdateTime"]) : DateTime.Now;
                    
                    logs.Add(new JourneyLog
                    {
                        Status = row["Status"]?.ToString() ?? "Update",
                        Location = row["Location"]?.ToString() ?? "In Transit",
                        FormattedTime = updateTime.ToString("MM/dd/yyyy HH:mm"),
                        IsLatest = (count == 0) // First item (newest) is the latest
                    });
                    count++;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    JourneyCollection.ItemsSource = logs;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"History Error: {ex.Message}");
            }
        }

        private void UpdateStatusUI(string status)
        {
            try
            {
                // Safety check for UI elements
                if (Step1Dot == null || CurrentStatusLabel == null) return;

                var gray = Color.FromArgb("#E2E8F0");
                var active = Color.FromArgb("#6366F1");

                // Reset dots
                Step1Dot.Fill = gray;
                if (Step2Dot != null) Step2Dot.Fill = gray;
                if (Step3Dot != null) Step3Dot.Fill = gray;
                if (Step4Dot != null) Step4Dot.Fill = gray;

                string statusLower = (status ?? "Pending").ToLower();

                if (statusLower.Contains("placed") || statusLower == "pending")
                {
                    Step1Dot.Fill = active;
                    CurrentStatusLabel.Text = "Order Received";
                }
                else if (statusLower.Contains("preparing") || statusLower == "paid" || statusLower == "confirmed")
                {
                    Step1Dot.Fill = active;
                    if (Step2Dot != null) Step2Dot.Fill = active;
                    CurrentStatusLabel.Text = "Processing & Packing";
                }
                else if (statusLower.Contains("partner") || statusLower.Contains("dropoff") || statusLower.Contains("facility") || statusLower.Contains("transit") || statusLower.Contains("delivery") || statusLower.Contains("shipped") || statusLower.Contains("departed") || statusLower.Contains("hub") || statusLower.Contains("truck"))
                {
                    Step1Dot.Fill = active;
                    if (Step2Dot != null) Step2Dot.Fill = active;
                    if (Step3Dot != null) Step3Dot.Fill = active;
                    CurrentStatusLabel.Text = status; // Shows specific step like "Out for Delivery"
                }
                else if (statusLower.Contains("arrived") || statusLower.Contains("delivered") || statusLower.Contains("completed") || statusLower.Contains("success"))
                {
                    Step1Dot.Fill = active;
                    if (Step2Dot != null) Step2Dot.Fill = active;
                    if (Step3Dot != null) Step3Dot.Fill = active;
                    if (Step4Dot != null) Step4Dot.Fill = active;
                    CurrentStatusLabel.Text = "Parcel Delivered";

                    // Show feedback prompt
                    if (FeedbackPromptButton != null) FeedbackPromptButton.IsVisible = true;
                    
                    // Auto-trigger modal once if not already shown this session for this order
                    string sessionKey = $"FeedbackShown_{UserSession.SelectedOrderRef}";
                    if (!Preferences.Default.ContainsKey(sessionKey))
                    {
                        FeedbackModalOverlay.IsVisible = true;
                        Preferences.Default.Set(sessionKey, true);
                    }
                }
                else
                {
                    Step1Dot.Fill = active;
                    CurrentStatusLabel.Text = status;
                    if (FeedbackPromptButton != null) FeedbackPromptButton.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateStatusUI Error: {ex.Message}");
            }
        }

        private void OnShowFeedbackModalClicked(object sender, EventArgs e) => FeedbackModalOverlay.IsVisible = true;
        private void OnCloseFeedbackModalClicked(object sender, EventArgs e) => FeedbackModalOverlay.IsVisible = false;

        private async void OnSubmitModalFeedbackClicked(object sender, EventArgs e)
        {
            string subject = FeedbackSubjectEntry.Text ?? "Delivery Feedback";
            string message = FeedbackMessageEditor.Text ?? "";

            if (string.IsNullOrWhiteSpace(message))
            {
                await DisplayAlertAsync("Feedback Required", "Please enter your message.", "OK");
                return;
            }

            int result = await _databaseService.SubmitFeedbackAsync(UserSession.UserEmail ?? "customer@aviah.com", subject, message);
            if (result > 0)
            {
                await DisplayAlertAsync("Thank You", "Your feedback has been received!", "OK");
                FeedbackModalOverlay.IsVisible = false;
                FeedbackPromptButton.IsVisible = false; // Hide prompt after submission
            }
        }
    }
}
