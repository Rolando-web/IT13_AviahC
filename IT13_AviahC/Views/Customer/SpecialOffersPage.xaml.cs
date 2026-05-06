using IT13_AviahC.Models;

namespace IT13_AviahC.Views.Customer
{
    public partial class SpecialOffersPage : ContentPage
    {
        public SpecialOffersPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadOffers();
        }

        private void LoadOffers()
        {
            try
            {
                var offers = new List<OfferItem>
                {
                    new OfferItem { ItemName = "Violet Summer Dress", OriginalPrice = "$120.00", PromoPrice = "$90.00", DiscountPercent = "25% OFF", Savings = "You save: $30.00", ImageUrl = "dress.png" },
                    new OfferItem { ItemName = "Lavender Blouse", OriginalPrice = "$65.00", PromoPrice = "$52.00", DiscountPercent = "20% OFF", Savings = "You save: $13.00", ImageUrl = "shirt.png" },
                    new OfferItem { ItemName = "Plum Formal Trousers", OriginalPrice = "$85.00", PromoPrice = "$63.75", DiscountPercent = "25% OFF", Savings = "You save: $21.25", ImageUrl = "pants.png" },
                    new OfferItem { ItemName = "Silk Scarf (Lilac)", OriginalPrice = "$35.00", PromoPrice = "$26.25", DiscountPercent = "25% OFF", Savings = "You save: $8.75", ImageUrl = "hoodie.png" },
                    new OfferItem { ItemName = "Purple Cardigan", OriginalPrice = "$55.00", PromoPrice = "$41.25", DiscountPercent = "25% OFF", Savings = "You save: $13.75", ImageUrl = "longsleeve.png" },
                    new OfferItem { ItemName = "Lilac Midi Skirt", OriginalPrice = "$70.00", PromoPrice = "$56.00", DiscountPercent = "20% OFF", Savings = "You save: $14.00", ImageUrl = "pants.png" }
                };

                OffersCollection.ItemsSource = offers;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading offers: {ex.Message}");
            }
        }
    }
}
