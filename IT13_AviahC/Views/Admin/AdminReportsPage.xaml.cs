using System.Data;
using IT13_AviahC.Services;
using Microsoft.Maui.Controls.Shapes;

namespace IT13_AviahC.Views.Admin
{
    public partial class AdminReportsPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public AdminReportsPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadReports();
        }

        private async void LoadReports()
        {
            try
            {
                var stats = await _dbService.GetDashboardStatsAsync();
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    TotalRevenueLabel.Text = string.Format("₱{0:N0}", stats["TotalRevenue"]);
                    ItemsSoldLabel.Text = "1,245"; // Keep proxy or fetch if OrderItems exist
                    NewCustomersLabel.Text = stats["NewCustomers"].ToString();
                    ProfitMarginLabel.Text = "42.8%";
                });

                // Load Categories
                DataTable categories = await _dbService.GetSalesByCategoryAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateCategoryData(categories);
                });

                // Load Turnover
                DataTable inventory = await _dbService.GetAllInventoryAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateTurnoverList(inventory);
                });
                
                // Update Trend Paths
                DataTable trendData = await _dbService.GetSalesOverviewAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateTrends(trendData);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reports load error: {ex.Message}");
            }
        }

        private void UpdateCategoryData(DataTable dt)
        {
            CategoryLegend.Children.Clear();
            decimal total = 0;
            foreach (DataRow row in dt.Rows)
            {
                total += Convert.ToDecimal(row["TotalValue"]);
            }

            foreach (DataRow row in dt.Rows)
            {
                string category = row["Category"]?.ToString() ?? "Other";
                decimal val = Convert.ToDecimal(row["TotalValue"]);
                double percentage = total > 0 ? (double)(val / total * 100) : 0;
                
                CategoryLegend.Add(new Label { Text = $"{category}: {percentage:F1}%", TextColor = Color.FromArgb("#64748B"), FontSize = 13 });
            }
        }

        private void UpdateTurnoverList(DataTable dt)
        {
            TurnoverList.Children.Clear();
            int count = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (count >= 3) break;
                string name = row["ProductName"]?.ToString() ?? "Unknown";
                int stock = Convert.ToInt32(row["StockQuantity"]);
                
                var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
                var stack = new VerticalStackLayout { Spacing = 4 };
                stack.Add(new Label { Text = name, FontAttributes = FontAttributes.Bold, FontSize = 14, TextColor = Color.FromArgb("#1E293B") });
                stack.Add(new Label { Text = $"Current Stock: {stock}", FontSize = 11, TextColor = Color.FromArgb("#94A3B8") });
                
                var rateStack = new VerticalStackLayout { HorizontalOptions = LayoutOptions.End, Spacing = 4 };
                rateStack.Add(new Label { Text = "8.5x", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1E293B"), HorizontalOptions = LayoutOptions.End });
                rateStack.Add(new Label { Text = "Normal", FontSize = 11, TextColor = Color.FromArgb("#94A3B8"), HorizontalOptions = LayoutOptions.End });
                
                grid.Add(stack, 0);
                grid.Add(rateStack, 1);
                TurnoverList.Add(grid);
                count++;
            }
        }

        private void UpdateTrends(DataTable dt)
        {
            if (dt.Rows.Count < 2) return;
            
            // Build SVG path data for the sales trend
            string pathData = "M0,300";
            decimal maxVal = 0;
            foreach (DataRow row in dt.Rows)
            {
                decimal val = Convert.ToDecimal(row["Total"]);
                if (val > maxVal) maxVal = val;
            }
            
            if (maxVal == 0) maxVal = 1000;
            
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                decimal val = Convert.ToDecimal(dt.Rows[i]["Total"]);
                double y = 300 - (double)(val / maxVal * 200);
                double x = (i * 700.0) / (dt.Rows.Count - 1);
                pathData += $" L{x:F0},{y:F0}";
            }
            
            SalesTrendPath.Data = new Microsoft.Maui.Controls.Shapes.PathGeometryConverter().ConvertFromInvariantString(pathData) as Microsoft.Maui.Controls.Shapes.Geometry;
            
            // Simpler profit path (roughly 40% of sales)
            string profitPath = "M0,300";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                decimal val = Convert.ToDecimal(dt.Rows[i]["Total"]) * 0.4m;
                double y = 300 - (double)(val / maxVal * 200);
                double x = (i * 700.0) / (dt.Rows.Count - 1);
                profitPath += $" L{x:F0},{y:F0}";
            }
            ProfitTrendPath.Data = new Microsoft.Maui.Controls.Shapes.PathGeometryConverter().ConvertFromInvariantString(profitPath) as Microsoft.Maui.Controls.Shapes.Geometry;
        }
    }
}
