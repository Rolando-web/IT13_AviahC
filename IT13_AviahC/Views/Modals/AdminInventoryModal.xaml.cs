using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace IT13_AviahC.Views.Modals
{
    public partial class AdminInventoryModal : ContentView
    {
        public event EventHandler? CloseRequested;
        public event EventHandler? SaveRequested;
        public event EventHandler? UploadImageRequested;

        public AdminInventoryModal()
        {
            InitializeComponent();
        }

        public void ShowForNew()
        {
            ModalTitle.Text = "Add Warehouse Item";
            ItemImagePreview.Source = null;
            ItemNameEntry.Text = string.Empty;
            SKUEntry.Text = string.Empty;
            CategoryEntry.Text = string.Empty;
            QuantityEntry.Text = string.Empty;
            UnitEntry.Text = "pcs";
            StatusPicker.SelectedIndex = 0;
        }

        public void ShowForEdit(string itemName, string sku, string category, int quantity, string? unit, string? status, string? imagePath)
        {
            ModalTitle.Text = "Edit Warehouse Item";
            ItemImagePreview.Source = string.IsNullOrEmpty(imagePath) ? null : ImageSource.FromFile(imagePath);
            ItemNameEntry.Text = itemName;
            SKUEntry.Text = sku;
            CategoryEntry.Text = category;
            QuantityEntry.Text = quantity.ToString();
            UnitEntry.Text = string.IsNullOrEmpty(unit) ? "pcs" : unit;
            StatusPicker.SelectedItem = string.IsNullOrEmpty(status) ? "In Stock" : status;
        }

        public string? ItemName => ItemNameEntry?.Text;
        public string? SKU => SKUEntry?.Text;
        public string Category => CategoryEntry?.Text ?? "General";
        public int Quantity => int.TryParse(QuantityEntry?.Text, out int qty) ? qty : 0;
        public string Unit => string.IsNullOrWhiteSpace(UnitEntry?.Text) ? "pcs" : UnitEntry?.Text ?? "pcs";
        public string Status => StatusPicker?.SelectedItem?.ToString() ?? "In Stock";
        public string? ImagePath { get; set; }

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
