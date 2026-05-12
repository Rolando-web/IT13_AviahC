using Microsoft.Maui.Controls;
using IT13_AviahC.Services;
using System.Data;

namespace IT13_AviahC.Views.Customer
{
    public partial class UserFeedbackPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public UserFeedbackPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadFeedbackHistoryAsync();
        }

        private async Task LoadFeedbackHistoryAsync()
        {
            try
            {
                string userEmail = UserSession.UserEmail ?? "customer@aviah.com";
                DataTable dt = await _databaseService.GetFeedbackHistoryAsync(userEmail);

                var history = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    history.Add(new
                    {
                        OrderRef = row["OrderRef"]?.ToString() ?? "N/A",
                        Comments = row["Comments"]?.ToString() ?? "",
                        DateSubmitted = row["DateSubmitted"] != DBNull.Value ? Convert.ToDateTime(row["DateSubmitted"]) : DateTime.Now
                    });
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    FeedbackHistoryCollection.ItemsSource = history;
                    EmptyHistoryView.IsVisible = history.Count == 0;
                    FeedbackHistoryCollection.IsVisible = history.Count > 0;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"History Error: {ex.Message}");
            }
        }
    }
}
