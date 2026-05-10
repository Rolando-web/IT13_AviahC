using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace IT13_AviahC.Views.Staff.Components;

public partial class StaffSidebarView : ContentView, INotifyPropertyChanged
{
    public static readonly BindableProperty CurrentPageProperty =
        BindableProperty.Create(nameof(CurrentPage), typeof(string), typeof(StaffSidebarView), string.Empty, propertyChanged: OnCurrentPageChanged);

    public string CurrentPage
    {
        get => (string)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public Color DashboardBg => CurrentPage == "StaffDashboard" ? Color.FromArgb("#463366") : Colors.Transparent;
    public Color InventoryBg => CurrentPage == "StaffInventory" ? Color.FromArgb("#463366") : Colors.Transparent;
    public Color ProductionBg => CurrentPage == "StaffProduction" ? Color.FromArgb("#463366") : Colors.Transparent;
    public Color SalesBg => CurrentPage == "StaffSales" ? Color.FromArgb("#463366") : Colors.Transparent;

    public Color DashboardColor => CurrentPage == "StaffDashboard" ? Colors.White : Color.FromArgb("#D1C4E9");
    public Color InventoryColor => CurrentPage == "StaffInventory" ? Colors.White : Color.FromArgb("#D1C4E9");
    public Color ProductionColor => CurrentPage == "StaffProduction" ? Colors.White : Color.FromArgb("#D1C4E9");
    public Color SalesColor => CurrentPage == "StaffSales" ? Colors.White : Color.FromArgb("#D1C4E9");

    public StaffSidebarView()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private static void OnCurrentPageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var sidebar = (StaffSidebarView)bindable;
        sidebar.OnPropertyChanged(nameof(DashboardBg));
        sidebar.OnPropertyChanged(nameof(InventoryBg));
        sidebar.OnPropertyChanged(nameof(ProductionBg));
        sidebar.OnPropertyChanged(nameof(SalesBg));
        sidebar.OnPropertyChanged(nameof(DashboardColor));
        sidebar.OnPropertyChanged(nameof(InventoryColor));
        sidebar.OnPropertyChanged(nameof(ProductionColor));
        sidebar.OnPropertyChanged(nameof(SalesColor));
    }

    private async void OnDashboardClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//StaffDashboard");
    private async void OnInventoryClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//StaffInventory");
    private async void OnProductionClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//StaffProduction");
    private async void OnSalesClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//StaffSales");

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await Shell.Current.DisplayAlertAsync("Logout", "Are you sure you want to logout?", "Yes", "No");
        if (confirm)
        {
            await Shell.Current.GoToAsync("//Login");
        }
    }
}
