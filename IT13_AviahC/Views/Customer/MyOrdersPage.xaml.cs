using System.Data;
using IT13_AviahC.Models;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Customer
{
    public partial class MyOrdersPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private string _currentOrderRef = string.Empty;

        public MyOrdersPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadOrdersSafe();
        }

        private async Task LoadOrdersSafe()
        {
            try
            {
                string userEmail = UserSession.UserEmail ?? "Customer@aviah.com";
                DataTable dt = await _databaseService.GetCustomerOrdersAsync(userEmail);
                var orders = new List<OrderItem>();

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string status = row["Status"]?.ToString() ?? "Pending";
                        DateTime orderDate = row["OrderDate"] != DBNull.Value ? Convert.ToDateTime(row["OrderDate"]) : DateTime.Now;
                        decimal amount = row["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalAmount"]) : 0m;

                        orders.Add(new OrderItem
                        {
                            OrderRef = row["OrderRef"]?.ToString() ?? "N/A",
                            OrderDateAndSummary = $"{orderDate:MMM dd, yyyy} • {row["ItemSummary"]}",
                            FormattedTotal = $"₱{amount:N2}",
                            Status = status,
                            StatusColor = GetStatusColor(status),
                            IsTrackable = status.ToLower() != "completed" && status.ToLower() != "cancelled",
                            HasFeedback = row.Table.Columns.Contains("HasFeedback") && row["HasFeedback"] != DBNull.Value && Convert.ToInt32(row["HasFeedback"]) == 1
                        });
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (orders.Count > 0)
                    {
                        OrdersCollection.IsVisible = true;
                        EmptyOrdersView.IsVisible = false;
                    }
                    else
                    {
                        OrdersCollection.IsVisible = false;
                        EmptyOrdersView.IsVisible = true;
                    }

                    OrdersCollection.ItemsSource = orders;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading orders: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OrdersCollection.IsVisible = false;
                    EmptyOrdersView.IsVisible = true;
                });
            }
        }

        private string GetStatusColor(string status)
        {
            return status.ToLower() switch
            {
                "pending" => "#805AD5",
                "paid" => "#3182CE",
                "shipped" => "#3182CE",
                "delivering" => "#DD6B20",
                "completed" => "#38A169",
                "cancelled" => "#E53E3E",
                _ => "#718096"
            };
        }

        private void OnTrackClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is OrderItem item)
            {
                UserSession.SelectedOrderRef = item.OrderRef;
                
                MainThread.BeginInvokeOnMainThread(async () => 
                {
                    try { await Shell.Current.GoToAsync("///CustomerTrack"); }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Track Nav Error: {ex.Message}"); }
                });
            }
        }

        private void OnFeedbackClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is OrderItem item)
            {
                _currentOrderRef = item.OrderRef;
                ModalTitle.Text = $"Review Order: {item.OrderRef}";
                FeedbackModalOverlay.IsVisible = true;
            }
        }

        private void OnCloseFeedbackModalClicked(object sender, EventArgs e)
        {
            FeedbackModalOverlay.IsVisible = false;
        }

        private async void OnSubmitModalFeedbackClicked(object sender, EventArgs e)
        {
            string subject = FeedbackSubjectEntry.Text ?? "Order Feedback";
            string message = FeedbackMessageEditor.Text ?? "";

            if (string.IsNullOrWhiteSpace(message))
            {
                await DisplayAlertAsync("Feedback Required", "Please enter your message.", "OK");
                return;
            }

            int result = await _databaseService.SubmitFeedbackAsync(UserSession.UserEmail ?? "customer@aviah.com", _currentOrderRef, subject, message);
            if (result > 0)
            {
                await DisplayAlertAsync("Thank You", "Your feedback has been received!", "OK");
                FeedbackModalOverlay.IsVisible = false;
                FeedbackSubjectEntry.Text = string.Empty;
                FeedbackMessageEditor.Text = string.Empty;
                _ = LoadOrdersSafe(); // Refresh to hide feedback button
            }
        }

        private async void OnShopClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("///CustomerBoutique");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shop Navigation Error: {ex.Message}");
            }
        }
    }
}
