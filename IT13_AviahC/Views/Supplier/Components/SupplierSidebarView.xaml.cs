using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace IT13_AviahC.Views.Supplier.Components;

public partial class SupplierSidebarView : ContentView, INotifyPropertyChanged
{
    public static readonly BindableProperty CurrentPageProperty =
        BindableProperty.Create(nameof(CurrentPage), typeof(string), typeof(SupplierSidebarView), string.Empty, propertyChanged: OnCurrentPageChanged);

    public string CurrentPage
    {
        get => (string)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public Color DashboardBg => CurrentPage == "SupplierDashboard" ? Color.FromArgb("#463366") : Colors.Transparent;
    public Color InventoryBg => CurrentPage == "SupplierInventory" ? Color.FromArgb("#463366") : Colors.Transparent;

    public Color DashboardColor => CurrentPage == "SupplierDashboard" ? Colors.White : Color.FromArgb("#D1C4E9");
    public Color InventoryColor => CurrentPage == "SupplierInventory" ? Colors.White : Color.FromArgb("#D1C4E9");

    public SupplierSidebarView()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private static void OnCurrentPageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var sidebar = (SupplierSidebarView)bindable;
        sidebar.OnPropertyChanged(nameof(DashboardBg));
        sidebar.OnPropertyChanged(nameof(InventoryBg));
        sidebar.OnPropertyChanged(nameof(DashboardColor));
        sidebar.OnPropertyChanged(nameof(InventoryColor));
    }

    private async void OnDashboardClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//SupplierDashboard");
    private async void OnInventoryClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//SupplierInventory");

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await Shell.Current.DisplayAlertAsync("Logout", "Are you sure you want to logout?", "Yes", "No");
        if (confirm)
        {
            await Shell.Current.GoToAsync("//Login");
        }
    }
}
