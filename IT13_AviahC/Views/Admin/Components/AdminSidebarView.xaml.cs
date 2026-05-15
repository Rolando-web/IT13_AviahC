using Microsoft.Maui.Controls;
using System.Windows.Input;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin.Components
{
    public partial class AdminSidebarView : ContentView
    {
        public static readonly BindableProperty CurrentPageProperty =
            BindableProperty.Create(nameof(CurrentPage), typeof(string), typeof(AdminSidebarView), string.Empty);

        public string CurrentPage
        {
            get => (string)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        private bool _isNavigating;
        public ICommand NavigateCommand { get; }

        public AdminSidebarView()
        {
            NavigateCommand = new Command<object>(async (param) => 
            {
                if (_isNavigating) return;
                
                try
                {
                    _isNavigating = true;
                    string route = param as string ?? string.Empty;
                    System.Diagnostics.Debug.WriteLine($"[AdminSidebar] Attempting navigation to: {route}");
                    
                    if (string.IsNullOrEmpty(route)) return;
                    
                    // Use // for reliable sibling navigation within the Shell tree
                    await Shell.Current.GoToAsync($"//{route}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminSidebar] Navigation error for {param}: {ex.Message}");
                }
                finally
                {
                    _isNavigating = false;
                }
            });
            InitializeComponent();
            this.Loaded += (s, e) => Refresh();
            ApplyTierRestrictions();
        }

        public void Refresh()
        {
            ApplyTierRestrictions();
        }

        private void ApplyTierRestrictions()
        {
            string tier = (UserSession.CurrentTier ?? "Basic").Trim();

            // Initialize all to visible then hide based on tier
            ProductionNav.IsVisible = true;
            SuppliersNav.IsVisible = true;
            LogisticsNav.IsVisible = true;
            ReportsNav.IsVisible = true;
            PromotionsNav.IsVisible = true;
            SubscriptionNav.IsVisible = true;
            ManageUsersNav.IsVisible = true;

            if (tier.Equals("Basic", StringComparison.OrdinalIgnoreCase))
            {
                ProductionNav.IsVisible = false;
                SuppliersNav.IsVisible = false;
                LogisticsNav.IsVisible = false;
                ReportsNav.IsVisible = false;
                PromotionsNav.IsVisible = false;
                ManageUsersNav.IsVisible = false;
                // Basic can see Dashboard, Inventory, Sales, Customers, Subscription
            }
            else if (tier.Equals("Standard", StringComparison.OrdinalIgnoreCase))
            {
                // Standard adds Production, Suppliers, Reports, Manage Users (Company)
                LogisticsNav.IsVisible = false;
                PromotionsNav.IsVisible = false;
            }
            // Premium (Tier 3) sees everything
        }
    }
}
