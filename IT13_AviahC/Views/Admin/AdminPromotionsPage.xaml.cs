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
    }
}
