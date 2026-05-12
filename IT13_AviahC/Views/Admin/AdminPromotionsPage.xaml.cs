using System.Data;
using System.Collections.ObjectModel;
using IT13_AviahC.Services;
using IT13_AviahC.Models;

namespace IT13_AviahC.Views.Admin;

public partial class AdminPromotionsPage : ContentPage
{
    private readonly DatabaseService _db;
    private Product? _selectedProduct;
    public ObservableCollection<Product> Products { get; } = new();

    public AdminPromotionsPage()
    {
        InitializeComponent();
        _db = new DatabaseService();
        ProductsCollection.ItemsSource = Products;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            DataTable dt = await _db.GetAllInventoryAsync();
            var list = new List<Product>();
            int promoCount = 0;

            foreach (DataRow row in dt.Rows)
            {
                var product = new Product
                {
                    Id = Convert.ToInt32(row["ProductID"]),
                    ProductName = row["ProductName"]?.ToString(),
                    Category = row["Category"]?.ToString(),
                    UnitPrice = Convert.ToDecimal(row["Price"]),
                    DiscountPrice = row["DiscountPrice"] != DBNull.Value ? Convert.ToDecimal(row["DiscountPrice"]) : (decimal?)null,
                    IsOnPromotion = row["IsOnPromotion"] != DBNull.Value && Convert.ToBoolean(row["IsOnPromotion"]),
                    ImageUrl = row["ImageUrl"]?.ToString() ?? "dress.png"
                };

                list.Add(product);
                if (product.IsOnPromotion) promoCount++;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Products.Clear();
                foreach (var p in list) Products.Add(p);
                PromoCountLabel.Text = $"{promoCount} Active Promos";
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Promo Load Error: {ex.Message}");
        }
    }

    private void OnApplyPromoClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Product product)
        {
            _selectedProduct = product;
            ModalTitleLabel.Text = $"Promotion: {product.ProductName}";
            RegularPriceLabel.Text = product.FormattedPrice;
            DiscountPriceEntry.Text = product.DiscountPrice?.ToString() ?? "";
            
            PromoModal.IsVisible = true;
        }
    }

    private async void OnRemovePromoClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Product product)
        {
            bool confirm = await DisplayAlert("Remove Promotion", $"Restore {product.ProductName} to original price of {product.UnitPrice:₱#,##0.00}?", "Yes", "No");
            if (confirm)
            {
                int res = await _db.SetProductPromotionAsync(product.Id, false, null);
                if (res > 0)
                {
                    await DisplayAlert("Success", "Product restored to Boutique.", "OK");
                    await LoadProductsAsync();
                }
            }
        }
    }

    private void OnCloseModalClicked(object sender, EventArgs e)
    {
        PromoModal.IsVisible = false;
        _selectedProduct = null;
    }

    private async void OnSavePromoClicked(object sender, EventArgs e)
    {
        if (_selectedProduct == null) return;

        if (decimal.TryParse(DiscountPriceEntry.Text, out decimal discountPrice))
        {
            if (discountPrice >= _selectedProduct.UnitPrice)
            {
                await DisplayAlert("Invalid Price", "Discounted price must be lower than the regular price.", "OK");
                return;
            }

            int result = await _db.SetProductPromotionAsync(_selectedProduct.Id, true, discountPrice);
            if (result > 0)
            {
                await DisplayAlert("Success", "Promotion applied successfully!", "OK");
                PromoModal.IsVisible = false;
                await LoadProductsAsync();
            }
            else
            {
                await DisplayAlert("Error", "Could not apply promotion.", "OK");
            }
        }
        else
        {
            await DisplayAlert("Error", "Please enter a valid numeric discount price.", "OK");
        }
    }
}
