using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace IT13_AviahC.Views.DeliveryStaff.Components;

public partial class DeliveryStaffSidebarView : ContentView, INotifyPropertyChanged
{
    public static readonly BindableProperty CurrentPageProperty =
        BindableProperty.Create(nameof(CurrentPage), typeof(string), typeof(DeliveryStaffSidebarView), string.Empty, propertyChanged: OnCurrentPageChanged);

    public string CurrentPage
    {
        get => (string)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public Color DashboardBg => CurrentPage == "DeliveryStaffDashboard" ? Color.FromArgb("#463366") : Colors.Transparent;
    public Color LogisticsBg => CurrentPage == "DeliveryStaffLogistics" ? Color.FromArgb("#463366") : Colors.Transparent;

    public Color DashboardColor => CurrentPage == "DeliveryStaffDashboard" ? Colors.White : Color.FromArgb("#D1C4E9");
    public Color LogisticsColor => CurrentPage == "DeliveryStaffLogistics" ? Colors.White : Color.FromArgb("#D1C4E9");

    public DeliveryStaffSidebarView()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private static void OnCurrentPageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var sidebar = (DeliveryStaffSidebarView)bindable;
        sidebar.OnPropertyChanged(nameof(DashboardBg));
        sidebar.OnPropertyChanged(nameof(LogisticsBg));
        sidebar.OnPropertyChanged(nameof(DashboardColor));
        sidebar.OnPropertyChanged(nameof(LogisticsColor));
    }

    private async void OnDashboardClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("///DeliveryStaffDashboard");
    private async void OnLogisticsClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("///DeliveryStaffLogistics");

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await Shell.Current.DisplayAlertAsync("Logout", "Are you sure you want to logout?", "Yes", "No");
        if (confirm)
        {
            await Shell.Current.GoToAsync("//Login");
        }
    }
}
