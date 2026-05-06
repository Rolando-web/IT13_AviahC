using Microsoft.Maui.Controls;

namespace IT13_AviahC.Views.Customer.Components
{
    public partial class CustomerHeaderView : ContentView
    {
        public static readonly BindableProperty ActiveTabProperty =
            BindableProperty.Create(nameof(ActiveTab), typeof(string), typeof(CustomerHeaderView), "Boutique", propertyChanged: OnActiveTabChanged);

        public string ActiveTab
        {
            get => (string)GetValue(ActiveTabProperty);
            set => SetValue(ActiveTabProperty, value);
        }

        public CustomerHeaderView()
        {
            InitializeComponent();
            UpdateVisualStates();
        }

        private static void OnActiveTabChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (CustomerHeaderView)bindable;
            view.UpdateVisualStates();
        }

        private void UpdateVisualStates()
        {
            if (BoutiqueBtn != null) VisualStateManager.GoToState(BoutiqueBtn, ActiveTab == "Boutique" ? "Selected" : "Normal");
            if (OffersBtn != null) VisualStateManager.GoToState(OffersBtn, ActiveTab == "Offers" ? "Selected" : "Normal");
            if (OrdersBtn != null) VisualStateManager.GoToState(OrdersBtn, ActiveTab == "Orders" ? "Selected" : "Normal");
            if (TrackBtn != null) VisualStateManager.GoToState(TrackBtn, ActiveTab == "Track" ? "Selected" : "Normal");
            if (FeedbackBtn != null) VisualStateManager.GoToState(FeedbackBtn, ActiveTab == "Feedback" ? "Selected" : "Normal");
        }

        private async void OnNavClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string route)
            {
                string shellRoute = route switch
                {
                    "Boutique" => "DirectBoutique",
                    "Offers" => "DirectOffers",
                    "Orders" => "DirectOrders",
                    _ => null
                };

                if (shellRoute != null)
                {
                    await Shell.Current.GoToAsync(shellRoute);
                }
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            if (Shell.Current?.CurrentPage != null)
            {
                bool answer = await Shell.Current.CurrentPage.DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
                if (answer)
                {
                    await Shell.Current.GoToAsync("///Login");
                }
            }
        }
    }
}
