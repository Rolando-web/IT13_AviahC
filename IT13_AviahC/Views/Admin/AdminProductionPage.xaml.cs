using System.Data;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin
{
    public partial class AdminProductionPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public AdminProductionPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadProduction();
        }

        private async void LoadProduction()
        {
            try
            {
                DataTable dt = _dbService.GetAllProduction();
                var batches = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    string status = row["Status"]?.ToString() ?? string.Empty;
                    bool isCompleted = status == "Completed";
                    
                    string statusColorDark = isCompleted ? "#15803D" : "#624890";
                    string statusColorLight = isCompleted ? "#DCFCE7" : "#F3F0FA";
                    string iconData = isCompleted ? "M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z" : "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z";
                    
                    double progress = Convert.ToDouble(row["Progress"]);
                    double progressWidth = (progress / 100.0) * 180.0;

                    batches.Add(new
                    {
                        BatchID = row["BatchID"]?.ToString() ?? string.Empty,
                        ProductName = row["ProductName"]?.ToString() ?? string.Empty,
                        Status = status,
                        StatusColorDark = statusColorDark,
                        StatusColorLight = statusColorLight,
                        IconData = iconData,
                        DateText = isCompleted ? $"Completed {row["EndDate"]}" : $"Started {row["StartDate"]}",
                        TargetText = $"Target: {row["TargetQuantity"]} pcs",
                        ProgressText = $"Progress: {progress}%",
                        ProgressWidth = progressWidth,
                        IsActive = !isCompleted
                    });
                }

                ProductionCollection.ItemsSource = batches;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", "Failed to load production: " + ex.Message, "OK");
            }
        }
    }
}
