using System.Data;
using IT13_AviahC.Services;
using Microsoft.Maui.Controls.Shapes;

namespace IT13_AviahC.Views.Admin
{
    public partial class AdminDashboardPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public AdminDashboardPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            try 
            {
                UpdateTierIndicator();
                LoadDashboardStats();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Admin Dashboard on appearing error: {ex.Message}");
            }
        }

        private void UpdateTierIndicator()
        {
            string tier = UserSession.CurrentTier ?? "Basic";
            TierLabel.Text = $"{tier} Tier";
            
            // Set colors based on tier
            if (tier == "Basic")
            {
                TierIndicatorDot.Fill = Brush.Gray;
                TierLabel.TextColor = Color.FromArgb("#475569");
            }
            else if (tier == "Standard")
            {
                TierIndicatorDot.Fill = Brush.Blue;
                TierLabel.TextColor = Color.FromArgb("#2563EB");
            }
            else
            {
                TierIndicatorDot.Fill = Color.FromArgb("#FBBF24"); // Gold for Premium
                TierLabel.TextColor = Color.FromArgb("#463366");
            }
        }

        private async void LoadDashboardStats()
        {
            try
            {
                var stats = await _dbService.GetDashboardStatsAsync();
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MonthlySalesLabel.Text = string.Format("₱{0:N0}", stats["MonthlySales"]);
                    TotalOrdersLabel.Text = stats["TotalOrders"].ToString();
                    LowStockLabel.Text = stats["LowStock"].ToString();
                    
                    // Mock conversion rate logic or just keep a reasonable number
                    ConversionRateLabel.Text = "3.2%";
                });

                // Load Chart Data
                DataTable chartData = await _dbService.GetSalesOverviewAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateSalesChart(chartData);
                });

                // Load Popular Products
                DataTable products = await _dbService.GetPopularProductsAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdatePopularProducts(products);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard load error: {ex.Message}");
            }
        }

        private void UpdateSalesChart(DataTable dt)
        {
            var bars = new[] { Bar1, Bar2, Bar3, Bar4, Bar5, Bar6 };
            decimal maxVal = 0;
            foreach (DataRow row in dt.Rows)
            {
                decimal val = Convert.ToDecimal(row["Total"]);
                if (val > maxVal) maxVal = val;
            }

            if (maxVal == 0) maxVal = 1000; // Avoid division by zero

            for (int i = 0; i < bars.Length; i++)
            {
                if (i < dt.Rows.Count)
                {
                    decimal val = Convert.ToDecimal(dt.Rows[i]["Total"]);
                    double percentage = (double)(val / maxVal);
                    bars[i].HeightRequest = Math.Max(20, percentage * 200);
                }
                else
                {
                    bars[i].HeightRequest = 20;
                }
            }
        }

        private void UpdatePopularProducts(DataTable dt)
        {
            PopularProductsList.Children.Clear();
            foreach (DataRow row in dt.Rows)
            {
                string name = row["ProductName"]?.ToString() ?? "Unknown";
                decimal price = Convert.ToDecimal(row["Price"]);
                int stock = Convert.ToInt32(row["StockQuantity"]);
                
                var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 16 };
                
                var border = new Border { BackgroundColor = Color.FromArgb("#F3F0FA"), StrokeShape = new RoundRectangle { CornerRadius = 12 }, HeightRequest = 56, WidthRequest = 56, StrokeThickness = 0 };
                border.Content = new Label { Text = GetEmoji(name), FontSize = 24, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
                
                var info = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
                info.Add(new Label { Text = name, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#463366"), FontSize = 14 });
                info.Add(new Label { Text = $"₱{price:N0}", FontSize = 11, TextColor = Color.FromArgb("#94A3B8") });
                
                var stockInfo = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End };
                stockInfo.Add(new Label { Text = $"{stock} left", FontAttributes = FontAttributes.Bold, TextColor = stock < 10 ? Color.FromArgb("#B91C1C") : Color.FromArgb("#15803D"), FontSize = 12 });
                stockInfo.Add(new Label { Text = "Stock", FontSize = 10, TextColor = Color.FromArgb("#94A3B8"), HorizontalTextAlignment = TextAlignment.End });
                
                grid.Add(border, 0);
                grid.Add(info, 1);
                grid.Add(stockInfo, 2);
                
                PopularProductsList.Add(grid);
            }
        }

        private string GetEmoji(string productName)
        {
            if (productName.Contains("Dress")) return "👗";
            if (productName.Contains("Shirt") || productName.Contains("Blouse")) return "👕";
            if (productName.Contains("Pants") || productName.Contains("Trousers")) return "👖";
            if (productName.Contains("Top")) return "👚";
            return "📦";
        }
    }
}
