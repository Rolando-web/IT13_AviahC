using Microsoft.Maui.Controls;

namespace IT13_AviahC.Views.Customer.Components
{
    public partial class CustomerHeaderView : ContentView
    {
        // Static navigation guard shared across ALL header instances to prevent re-entrant navigation
        private static bool _isNavigating = false;

        public static readonly BindableProperty ActiveTabProperty =
            BindableProperty.Create(nameof(ActiveTab), typeof(string), typeof(CustomerHeaderView), "Boutique", propertyChanged: OnActiveTabChanged);

        public static readonly BindableProperty CustomerNameProperty =
            BindableProperty.Create(nameof(CustomerName), typeof(string), typeof(CustomerHeaderView), "Customer", propertyChanged: OnCustomerNameChanged);

        public string ActiveTab
        {
            get => (string)GetValue(ActiveTabProperty);
            set => SetValue(ActiveTabProperty, value);
        }

        public string CustomerName
        {
            get => (string)GetValue(CustomerNameProperty);
            set => SetValue(CustomerNameProperty, value);
        }

        public CustomerHeaderView()
        {
            InitializeComponent();
            CustomerName = Services.UserSession.UserName ?? "Customer";
            this.Loaded += (s, e) => UpdateVisualStates();
        }

        private static void OnActiveTabChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CustomerHeaderView view)
            {
                view.UpdateVisualStates();
            }
        }

        private static void OnCustomerNameChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CustomerHeaderView view && view.CustomerNameLabel != null)
            {
                view.CustomerNameLabel.Text = (string)newValue;
            }
        }

        private void UpdateVisualStates()
        {
            try
            {
                if (BoutiqueBtn != null) VisualStateManager.GoToState(BoutiqueBtn, ActiveTab == "Boutique" ? "Selected" : "Normal");
                if (OffersBtn != null) VisualStateManager.GoToState(OffersBtn, ActiveTab == "Offers" ? "Selected" : "Normal");
                if (OrdersBtn != null) VisualStateManager.GoToState(OrdersBtn, ActiveTab == "Orders" ? "Selected" : "Normal");
                if (TrackBtn != null) VisualStateManager.GoToState(TrackBtn, ActiveTab == "Track" ? "Selected" : "Normal");
                if (FeedbackBtn != null) VisualStateManager.GoToState(FeedbackBtn, ActiveTab == "Feedback" ? "Selected" : "Normal");

                if (BoutiqueIcon != null) BoutiqueIcon.Fill = new SolidColorBrush(ActiveTab == "Boutique" ? Color.Parse("#624890") : Color.Parse("#64748B"));
                if (OffersIcon != null) OffersIcon.Fill = new SolidColorBrush(ActiveTab == "Offers" ? Color.Parse("#624890") : Color.Parse("#64748B"));
                if (OrdersIcon != null) OrdersIcon.Fill = new SolidColorBrush(ActiveTab == "Orders" ? Color.Parse("#624890") : Color.Parse("#64748B"));
                if (TrackIcon != null) TrackIcon.Fill = new SolidColorBrush(ActiveTab == "Track" ? Color.Parse("#624890") : Color.Parse("#64748B"));
                if (FeedbackIcon != null) FeedbackIcon.Fill = new SolidColorBrush(ActiveTab == "Feedback" ? Color.Parse("#624890") : Color.Parse("#64748B"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateVisualStates Error: {ex.Message}");
            }
        }

        private async void OnNavClicked(object sender, EventArgs e)
        {
            // If already navigating globally, ignore the click entirely
            if (_isNavigating) return;

            if (sender is Button button && button.CommandParameter is string route)
            {
                // Don't navigate if we're already on this tab
                if (route == ActiveTab) return;

                string? shellRoute = route switch
                {
                    "Boutique" => "//CustomerPortal/CustomerBoutique",
                    "Offers" => "//CustomerPortal/CustomerOffers",
                    "Orders" => "//CustomerPortal/CustomerOrders",
                    "Track" => "//CustomerPortal/CustomerTrack",
                    "Feedback" => "//CustomerPortal/UserFeedback",
                    _ => null
                };

                if (shellRoute != null)
                {
                    _isNavigating = true;
                    try
                    {
                        await Shell.Current.GoToAsync(shellRoute);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Navigation Error: {ex.Message}");
                    }
                    finally
                    {
                        // Use Dispatcher to reset the flag after the UI settles
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(300);
                            _isNavigating = false;
                        });
                    }
                }
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            if (_isNavigating) return;

            try
            {
                if (Shell.Current?.CurrentPage != null)
                {
                    bool answer = await Shell.Current.CurrentPage.DisplayAlertAsync("Logout", "Are you sure you want to logout?", "Yes", "No");
                    if (answer)
                    {
                        _isNavigating = true;
                        try
                        {
                            Services.UserSession.UserName = null;
                            Services.UserSession.UserEmail = null;
                            Services.UserSession.Role = null;
                            await Shell.Current.GoToAsync("///Login");
                        }
                        finally
                        {
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(300);
                                _isNavigating = false;
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout Error: {ex.Message}");
                _isNavigating = false;
            }
        }
    }
}
