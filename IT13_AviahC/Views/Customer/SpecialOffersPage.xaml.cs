using System.Data;
using IT13_AviahC.Models;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Customer;

public partial class SpecialOffersPage : ContentPage
{
    private readonly DatabaseService _db;

    public SpecialOffersPage()
    {
        InitializeComponent();
        _db = new DatabaseService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadOffersAsync();
    }

    private async Task LoadOffersAsync()
    {
        try
        {
            DataTable dt = await _db.GetPromotedProductsAsync();
            var items = new List<Product>();

            foreach (DataRow row in dt.Rows)
            {
                items.Add(new Product
                {
                    Id = Convert.ToInt32(row["ProductID"]),
                    ProductName = row["ProductName"]?.ToString(),
                    Category = row["Category"]?.ToString(),
                    UnitPrice = Convert.ToDecimal(row["Price"]),
                    DiscountPrice = row["DiscountPrice"] != DBNull.Value ? Convert.ToDecimal(row["DiscountPrice"]) : (decimal?)null,
                    IsOnPromotion = true, // We fetched from GetPromotedProductsAsync
                    ImageUrl = row["ImageUrl"]?.ToString() ?? "dress.png"
                });
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (items.Count > 0)
                {
                    OffersCollection.IsVisible = true;
                    EmptyStateView.IsVisible = false;
                    OffersCollection.ItemsSource = items;
                }
                else
                {
                    OffersCollection.IsVisible = false;
                    EmptyStateView.IsVisible = true;
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Offers Load Error: {ex.Message}");
        }
    }

    private async void OnOrderClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Product product)
        {
            decimal finalPrice = product.DiscountPrice ?? product.UnitPrice;
            bool confirm = await DisplayAlertAsync("Confirm Purchase", $"Order {product.ProductName} for the special price of {finalPrice:₱#,##0.00}?", "Yes", "No");
            
            if (confirm)
            {
                string email = UserSession.UserEmail ?? "customer@aviah.com";
                int result = await _db.PlaceOrderAsync(email, product.ProductName ?? "Promo Item", finalPrice);
                
                if (result > 0)
                {
                    await DisplayAlertAsync("Order Placed!", "Your promotional item has been ordered successfully.", "OK");
                    await Shell.Current.GoToAsync("///CustomerOrders");
                }
            }
        }
    }
}
