using IT13_AviahC.Models;
using Microsoft.Maui.Controls;

namespace IT13_AviahC.Views.Modals
{
    public partial class StaffInventoryModal : ContentView
    {
        public event EventHandler? CloseRequested;
        public event EventHandler? SaveRequested;
        public event EventHandler? UploadImageRequested;

        public StaffInventoryModal()
        {
            InitializeComponent();
        }

        public void ShowForNew()
        {
            ModalTitle.Text = "Add New Product";
            ItemImagePreview.Source = null;
            ItemNameEntry.Text = string.Empty;
            SKUEntry.Text = string.Empty;
            CategoryPicker.SelectedIndex = -1;
            QuantityEntry.Text = string.Empty;
            WarehouseQuantityEntry.Text = string.Empty;
            PriceEntry.Text = string.Empty;
            PromotionPicker.SelectedItem = null;
        }

        public void ShowForEdit(Product item, IEnumerable<Promotion>? promotions)
        {
            ModalTitle.Text = "Edit Product";
            ItemImagePreview.Source = string.IsNullOrEmpty(item.ImageUrl) ? null : ImageSource.FromFile(item.ImageUrl);
            ItemNameEntry.Text = item.ProductName;
            SKUEntry.Text = item.SKU;
            CategoryPicker.SelectedItem = item.Category;
            QuantityEntry.Text = item.StockLevel.ToString();
            WarehouseQuantityEntry.Text = item.WarehouseStock.ToString();
            PriceEntry.Text = item.UnitPrice.ToString();

            if (promotions != null && item.PromoId.HasValue)
            {
                PromotionPicker.SelectedItem = promotions.FirstOrDefault(p => p.PromoID == item.PromoId);
            }
            else
            {
                PromotionPicker.SelectedItem = null;
            }
        }

        public string? ProductName => ItemNameEntry.Text;
        public string? SKU => SKUEntry.Text;
        public string Category => CategoryPicker.SelectedItem?.ToString() ?? "General";
        public int StockLevel => int.TryParse(QuantityEntry.Text, out int stock) ? stock : 0;
        public int WarehouseStock => int.TryParse(WarehouseQuantityEntry.Text, out int wStock) ? wStock : 0;
        public decimal UnitPrice => decimal.TryParse(PriceEntry.Text, out decimal price) ? price : 0m;
        public int? PromoId => (PromotionPicker.SelectedItem as Promotion)?.PromoID;
        public string? ImagePath { get; set; }

        public void SetPromotions(IEnumerable<Promotion> promotions)
        {
            PromotionPicker.ItemsSource = promotions.ToList();
        }

        public void UpdateImagePreview(string path)
        {
            ImagePath = path;
            ItemImagePreview.Source = string.IsNullOrEmpty(path) ? null : ImageSource.FromFile(path);
        }

        private void OnCloseRequested(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnSaveRequested(object sender, EventArgs e)
        {
            SaveRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnUploadImageClicked(object sender, EventArgs e)
        {
            UploadImageRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
