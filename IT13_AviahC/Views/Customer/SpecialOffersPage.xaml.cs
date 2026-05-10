using System.Data;
using IT13_AviahC.Models;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Customer
{
    public partial class SpecialOffersPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public SpecialOffersPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadOffersSafe();
        }

        private async Task LoadOffersSafe()
        {
            try
            {
                DataTable dt = await _databaseService.GetPromotionsAsync();
                var offers = new List<OfferItem>();

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        decimal originalPrice = row["Price"] != DBNull.Value ? Convert.ToDecimal(row["Price"]) : 0m;
                        string discountValue = row["DiscountValue"]?.ToString() ?? "";
                        decimal promoPrice = originalPrice;
                        
                        string val = discountValue.Replace("%", "").Replace("OFF", "").Trim();
                        if (decimal.TryParse(val, out decimal discountAmt))
                        {
                            if (discountValue.Contains("%"))
                                promoPrice = originalPrice * (1 - (discountAmt / 100));
                            else
                                promoPrice = Math.Max(0, originalPrice - discountAmt);
                        }

                        offers.Add(new OfferItem 
                        { 
                            ItemName = row["ProductName"]?.ToString() ?? "Special Item", 
                            OriginalPrice = $"₱{originalPrice:N2}", 
                            PromoPrice = $"₱{promoPrice:N2}", 
                            DiscountPercent = discountValue, 
                            Savings = $"You save: ₱{(originalPrice - promoPrice):N2}", 
                            ImageUrl = row["ImageUrl"]?.ToString() ?? "package_icon.png",
                            PromotionName = row["PromotionName"]?.ToString() ?? "Special Offer"
                        });
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (offers.Count > 0)
                    {
                        OffersCollection.IsVisible = true;
                        EmptyPromotionsView.IsVisible = false;
                    }
                    else
                    {
                        OffersCollection.IsVisible = false;
                        EmptyPromotionsView.IsVisible = true;
                    }

                    BindableLayout.SetItemsSource(OffersCollection, offers);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading offers: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OffersCollection.IsVisible = false;
                    EmptyPromotionsView.IsVisible = true;
                });
            }
        }
    }
}
