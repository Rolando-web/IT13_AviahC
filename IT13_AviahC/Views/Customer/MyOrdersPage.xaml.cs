using IT13_AviahC.Models;

namespace IT13_AviahC.Views.Customer
{
    public partial class MyOrdersPage : ContentPage
    {
        public MyOrdersPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                var orders = new List<OrderItem>
                {
                    new OrderItem 
                    { 
                        OrderRef = "ORD-983", 
                        OrderDateAndSummary = "Today • Lavender Blouse, Trousers", 
                        FormattedTotal = "$185.00", 
                        Status = "In Transit", 
                        StatusColor = "#3182CE", 
                        IsTrackable = true 
                    },
                    new OrderItem 
                    { 
                        OrderRef = "ORD-842", 
                        OrderDateAndSummary = "Oct 12, 2025 • Silk Scarf (Lilac)", 
                        FormattedTotal = "$35.00", 
                        Status = "Delivered", 
                        StatusColor = "#38A169", 
                        IsTrackable = false 
                    },
                    new OrderItem 
                    { 
                        OrderRef = "ORD-710", 
                        OrderDateAndSummary = "Sep 05, 2025 • Violet Summer Dress", 
                        FormattedTotal = "$120.00", 
                        Status = "Delivered", 
                        StatusColor = "#38A169", 
                        IsTrackable = false 
                    }
                };

                OrdersCollection.ItemsSource = orders;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading orders: {ex.Message}");
            }
        }
    }
}
