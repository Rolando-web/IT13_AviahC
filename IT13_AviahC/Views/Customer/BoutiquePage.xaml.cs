using IT13_AviahC.Models;

namespace IT13_AviahC.Views.Customer
{
    public partial class BoutiquePage : ContentPage
    {
        public BoutiquePage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadBoutiqueItems();
        }

        private void LoadBoutiqueItems()
        {
            try
            {
                var items = new List<BoutiqueItem>
                {
                    new BoutiqueItem { ItemName = "Violet Summer Dress", UnitPrice = 120.00m, FormattedPrice = "$120.00", ImageUrl = "dress.png" },
                    new BoutiqueItem { ItemName = "Lavender Blouse", UnitPrice = 65.00m, FormattedPrice = "$65.00", ImageUrl = "shirt.png" },
                    new BoutiqueItem { ItemName = "Plum Formal Trousers", UnitPrice = 85.00m, FormattedPrice = "$85.00", ImageUrl = "pants.png" },
                    new BoutiqueItem { ItemName = "Silk Scarf (Lilac)", UnitPrice = 35.00m, FormattedPrice = "$35.00", ImageUrl = "hoodie.png" },
                    new BoutiqueItem { ItemName = "Purple Cardigan", UnitPrice = 55.00m, FormattedPrice = "$55.00", ImageUrl = "longsleeve.png" },
                    new BoutiqueItem { ItemName = "Amethyst Earrings", UnitPrice = 45.00m, FormattedPrice = "$45.00", ImageUrl = "shirt.png" },
                    new BoutiqueItem { ItemName = "Lilac Midi Skirt", UnitPrice = 70.00m, FormattedPrice = "$70.00", ImageUrl = "pants.png" },
                    new BoutiqueItem { ItemName = "Violet Handbag", UnitPrice = 95.00m, FormattedPrice = "$95.00", ImageUrl = "hoodie.png" }
                };

                ItemsCollection.ItemsSource = items;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading boutique: {ex.Message}");
            }
        }
    }
}
