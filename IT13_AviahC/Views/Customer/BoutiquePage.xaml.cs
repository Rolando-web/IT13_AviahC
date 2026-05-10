using IT13_AviahC.Models;
using System.Data;

namespace IT13_AviahC.Views.Customer
{
    public partial class BoutiquePage : ContentPage
    {
        private readonly Services.DatabaseService _databaseService;

        public BoutiquePage()
        {
            InitializeComponent();
            _databaseService = new Services.DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadBoutiqueItemsSafe();
        }

        private async Task LoadBoutiqueItemsSafe()
        {
            try
            {
                DataTable dt = await _databaseService.GetAllInventoryAsync();
                var items = new List<Product>();

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string status = row["Status"]?.ToString() ?? "In Stock";
                        string category = row["Category"]?.ToString() ?? "General";
                        string productName = row["ProductName"]?.ToString() ?? "Unknown Item";
                        string? imageUrl = row["ImageUrl"]?.ToString();
                        
                        if (string.IsNullOrEmpty(imageUrl))
                        {
                            imageUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(productName)}&background=random&size=128";
                        }

                        items.Add(new Product
                        {
                            Id = row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0,
                            ProductName = productName,
                            UnitPrice = row["Price"] != DBNull.Value ? Convert.ToDecimal(row["Price"]) : 0m,
                            ImageUrl = imageUrl,
                            Category = category,
                            StatusText = row["PromotionName"]?.ToString() ?? status,
                            PromoId = row["PromoId"] != DBNull.Value ? Convert.ToInt32(row["PromoId"]) : (int?)null,
                            PromotionName = row["PromotionName"]?.ToString(),
                            DiscountValue = row["DiscountValue"]?.ToString()
                        });
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (items.Count > 0)
                    {
                        ItemsCollection.IsVisible = true;
                        EmptyBoutiqueView.IsVisible = false;
                    }
                    else
                    {
                        ItemsCollection.IsVisible = false;
                        EmptyBoutiqueView.IsVisible = true;
                    }
                    BindableLayout.SetItemsSource(ItemsCollection, items);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading boutique: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ItemsCollection.IsVisible = false;
                    EmptyBoutiqueView.IsVisible = true;
                });
            }
        }

        private async void OnOrderClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is Product product)
                {
                    bool confirm = await DisplayAlertAsync("Confirm Order", $"Would you like to order {product.ProductName} for {product.FormattedPrice}?", "Yes", "No");
                    
                    if (confirm)
                    {
                        string userEmail = Services.UserSession.UserEmail ?? "customer@aviah.com";
                        int result = await _databaseService.PlaceOrderAsync(userEmail, product.ProductName ?? "Item", product.UnitPrice);
                        
                        if (result > 0)
                        {
                            await DisplayAlertAsync("Success", "Your order has been placed successfully!", "OK");
                            await Shell.Current.GoToAsync("//CustomerPortal/CustomerOrders");
                        }
                        else
                        {
                            await DisplayAlertAsync("Error", "Failed to place order. Please try again.", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Order Error: {ex.Message}");
                await DisplayAlertAsync("Error", "Something went wrong while placing your order. Please try again.", "OK");
            }
        }
    }
}
