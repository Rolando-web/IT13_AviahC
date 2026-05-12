using Microsoft.Maui.Controls;

namespace IT13_AviahC.Views.Customer.Components
{
    public partial class CustomerHeaderView : ContentView
    {
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
            this.Loaded += (s, e) => {
                MainThread.BeginInvokeOnMainThread(() => UpdateVisualStates());
            };
        }

        private static void OnActiveTabChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CustomerHeaderView view)
            {
                MainThread.BeginInvokeOnMainThread(() => view.UpdateVisualStates());
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
                // Safety null checks for all components
                if (BoutiqueBtn == null) return; 

                VisualStateManager.GoToState(BoutiqueBtn, ActiveTab == "Boutique" ? "Selected" : "Normal");
                if (OffersBtn != null) VisualStateManager.GoToState(OffersBtn, ActiveTab == "Offers" ? "Selected" : "Normal");
                if (OrdersBtn != null) VisualStateManager.GoToState(OrdersBtn, ActiveTab == "Orders" ? "Selected" : "Normal");
                if (TrackBtn != null) VisualStateManager.GoToState(TrackBtn, ActiveTab == "Track" ? "Selected" : "Normal");
                if (FeedbackBtn != null) VisualStateManager.GoToState(FeedbackBtn, ActiveTab == "Feedback" ? "Selected" : "Normal");

                Color primary = Color.FromArgb("#624890");
                Color muted = Color.FromArgb("#64748B");

                if (BoutiqueIcon != null) BoutiqueIcon.Fill = new SolidColorBrush(ActiveTab == "Boutique" ? primary : muted);
                if (OffersIcon != null) OffersIcon.Fill = new SolidColorBrush(ActiveTab == "Offers" ? primary : muted);
                if (OrdersIcon != null) OrdersIcon.Fill = new SolidColorBrush(ActiveTab == "Orders" ? primary : muted);
                if (TrackIcon != null) TrackIcon.Fill = new SolidColorBrush(ActiveTab == "Track" ? primary : muted);
                if (FeedbackIcon != null) FeedbackIcon.Fill = new SolidColorBrush(ActiveTab == "Feedback" ? primary : muted);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateVisualStates Error: {ex.Message}");
            }
        }

        private async void OnNavClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string route)
            {
                if (route == ActiveTab) return;

                string? shellRoute = route switch
                {
                    "Boutique" => "///CustomerBoutique",
                    "Offers" => "///CustomerOffers",
                    "Orders" => "///CustomerOrders",
                    "Track" => "///CustomerTrack",
                    "Feedback" => "///UserFeedback",
                    _ => null
                };

                if (shellRoute != null)
                {
                    try
                    {
                        // Use MainThread to ensure navigation doesn't deadlock with current event cycle
                        MainThread.BeginInvokeOnMainThread(async () => 
                        {
                            try { await Shell.Current.GoToAsync(shellRoute); }
                            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Navigation Error: {ex.Message}"); }
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Critical Navigation Error: {ex.Message}");
                    }
                }
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            try
            {
                if (Shell.Current?.CurrentPage != null)
                {
                    bool answer = await Shell.Current.CurrentPage.DisplayAlertAsync("Logout", "Are you sure you want to logout?", "Yes", "No");
                    if (answer)
                    {
                        try
                        {
                            Services.UserSession.UserName = null;
                            Services.UserSession.UserEmail = null;
                            Services.UserSession.Role = null;
                            await Shell.Current.GoToAsync("///Login");
                        }
                        finally
                        {
                            await Task.Delay(500);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout Error: {ex.Message}");
            }
        }
    }
}
