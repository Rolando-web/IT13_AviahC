using System.Data;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin
{
    public partial class AdminPromotionsPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public AdminPromotionsPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadPromotions();
        }

        private async void LoadPromotions()
        {
            try
            {
                DataTable dt = _dbService.GetAllPromotions();
                var promotions = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    int usage = Convert.ToInt32(row["UsageCount"] ?? 0);
                    int max = Convert.ToInt32(row["MaxUsage"] ?? 0);
                    double progress = max > 0 ? (double)usage / max : 0;

                    promotions.Add(new
                    {
                        PromoCode = row["PromoCode"]?.ToString() ?? string.Empty,
                        PromotionName = row["PromotionName"]?.ToString() ?? string.Empty,
                        DiscountValue = row["DiscountValue"]?.ToString() ?? string.Empty,
                        TargetAudience = row["TargetAudience"]?.ToString() ?? string.Empty,
                        Duration = $"{Convert.ToDateTime(row["StartDate"] ?? DateTime.Now):yyyy-MM-dd} → {Convert.ToDateTime(row["EndDate"] ?? DateTime.Now):yyyy-MM-dd}",
                        UsageText = $"{usage} / {max}",
                        UsageProgress = progress,
                        Status = row["Status"]?.ToString() ?? string.Empty
                    });
                }

                ActivePromosCollection.ItemsSource = promotions;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", "Failed to load promotions: " + ex.Message, "OK");
            }
        }

        private void OnCreatePromotionClicked(object sender, EventArgs e)
        {
            // Reset form
            PromoCodeEntry.Text = string.Empty;
            PromoNameEntry.Text = string.Empty;
            PromoDiscountEntry.Text = string.Empty;
            PromoTargetEntry.Text = string.Empty;
            PromoStartDate.Date = DateTime.Now;
            PromoEndDate.Date = DateTime.Now.AddMonths(1);
            PromoStatusPicker.SelectedIndex = 0;

            PromoModal.IsVisible = true;
        }

        private void OnCloseModalClicked(object sender, EventArgs e)
        {
            PromoModal.IsVisible = false;
        }

        private async void OnSavePromotionClicked(object sender, EventArgs e)
        {
            string code = PromoCodeEntry.Text;
            string name = PromoNameEntry.Text;
            string discount = PromoDiscountEntry.Text;
            string target = PromoTargetEntry.Text;
            DateTime start = PromoStartDate.Date;
            DateTime end = PromoEndDate.Date;
            string status = PromoStatusPicker.SelectedItem?.ToString() ?? "Active";

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Validation Error", "Promo Code and Name are required.", "OK");
                return;
            }

            string query = @"
                INSERT INTO Promotions (PromoCode, PromotionName, DiscountValue, TargetAudience, Status, UsageCount, MaxUsage, StartDate, EndDate)
                VALUES (@Code, @Name, @Discount, @Target, @Status, 0, 1000, @Start, @End)";
            
            var parameters = new Dictionary<string, object>
            {
                { "@Code", code },
                { "@Name", name },
                { "@Discount", discount },
                { "@Target", target },
                { "@Status", status },
                { "@Start", start },
                { "@End", end }
            };

            int rows = await _dbService.ExecuteNonQueryAsync(query, parameters);
            if (rows > 0)
            {
                await DisplayAlert("Success", "Promotion created successfully.", "OK");
                PromoModal.IsVisible = false;
                LoadPromotions(); // Refresh table
            }
            else
            {
                await DisplayAlert("Error", "Failed to create promotion.", "OK");
            }
        }
    }
}
