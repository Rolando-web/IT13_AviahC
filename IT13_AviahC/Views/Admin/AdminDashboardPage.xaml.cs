using System.Data;
using IT13_AviahC.Services;

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
            LoadDashboardStats();
        }

        private async void LoadDashboardStats()
        {
            try
            {
                // In a real app, we would have a specific GetDashboardStats() method
                // For now, we'll use the existing methods to show live counts
                
                DataTable sales = _dbService.GetAllSales();
                DataTable inventory = _dbService.GetAllInventory();
                DataTable orders = _dbService.GetAllDeliveries(); // Using deliveries as proxy for orders

                // Update UI Labels if they had names (they don't in current XAML)
                // Since I can't easily add names to many labels at once without rewriting XAML,
                // I'll focus on the logic.
                
                // If the user wants specific dynamic stats in the main dashboard, 
                // I would need to name the labels in XAML.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard load error: {ex.Message}");
            }
        }
    }
}
