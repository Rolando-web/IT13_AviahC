using System.Data;
using System.Collections.ObjectModel;
using IT13_AviahC.Services;
using IT13_AviahC.Models;
using Microsoft.Maui.Controls;

namespace IT13_AviahC.Views.Staff;

public partial class StaffInventoryPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private int _editingItemId = -1;
    private string _selectedImagePath = "";
    public ObservableCollection<Product> InventoryItems { get; set; } = new ObservableCollection<Product>();

    public StaffInventoryPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        InventoryCollection.ItemsSource = InventoryItems;
        LoadPromotions();
    }

    private void LoadPromotions()
    {
        try
        {
            DataTable dt = _dbService.GetAllPromotions();
            var promoList = new List<Promotion>();
            foreach (DataRow row in dt.Rows)
            {
                promoList.Add(new Promotion
                {
                    PromoID = Convert.ToInt32(row["PromoID"]),
                    PromotionName = row["PromotionName"]?.ToString(),
                    PromoCode = row["PromoCode"]?.ToString()
                });
            }
            PromotionPicker.ItemsSource = promoList;
        }
        catch { }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadInventory();
    }

    private async void LoadInventory()
    {
        try
        {
            DataTable dt = await _dbService.GetAllInventoryAsync();
            InventoryItems.Clear();

            if (dt != null && dt.Rows.Count > 0)
            {
                EmptyStateView.IsVisible = false;
                InventoryCollection.IsVisible = true;

                foreach (DataRow row in dt.Rows)
                {
                    string img = row["ImageUrl"]?.ToString() ?? "";
                    InventoryItems.Add(new Product
                    {
                        Id = Convert.ToInt32(row["Id"] ?? 0),
                        ProductName = row["ProductName"]?.ToString() ?? "Unknown",
                        SKU = row["SKU"]?.ToString() ?? "N/A",
                        Category = row["Category"]?.ToString() ?? "General",
                        StockLevel = Convert.ToInt32(row["StockLevel"] ?? 0),
                        UnitPrice = row["Price"] != DBNull.Value ? Convert.ToDecimal(row["Price"]) : 0m,
                        StatusText = row["Status"]?.ToString() ?? "In Stock",
                        ImageUrl = string.IsNullOrEmpty(img) ? "package_icon.png" : img,
                        PromoId = row["PromoId"] != DBNull.Value ? Convert.ToInt32(row["PromoId"]) : (int?)null,
                        PromotionName = row["PromotionName"]?.ToString()
                    });
                }
            }
            else
            {
                InventoryCollection.IsVisible = false;
                EmptyStateView.IsVisible = true;
            }
        }
        catch { }
    }

    private async void OnUploadImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Product Image",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                _selectedImagePath = result.FullPath;
                ItemImagePreview.Source = ImageSource.FromFile(_selectedImagePath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "Could not pick image: " + ex.Message, "OK");
        }
    }

    private void OnAddItemClicked(object sender, EventArgs e)
    {
        _editingItemId = -1;
        _selectedImagePath = "";
        ItemImagePreview.Source = null;
        ModalTitle.Text = "Add New Product";
        ItemNameEntry.Text = "";
        SKUEntry.Text = "";
        CategoryPicker.SelectedIndex = -1;
        QuantityEntry.Text = "";
        PriceEntry.Text = "";
        PromotionPicker.SelectedItem = null;
        ItemModal.IsVisible = true;
    }

    private void OnEditItemClicked(object sender, EventArgs e)
    {
        if (sender is Label label && label.GestureRecognizers.Count > 0 && label.GestureRecognizers[0] is TapGestureRecognizer tap && tap.CommandParameter is Product item)
        {
            _editingItemId = item.Id;
            _selectedImagePath = item.ImageUrl;
            ItemImagePreview.Source = string.IsNullOrEmpty(_selectedImagePath) ? null : ImageSource.FromFile(_selectedImagePath);
            
            ModalTitle.Text = "Edit Product";
            ItemNameEntry.Text = item.ProductName;
            SKUEntry.Text = item.SKU;
            CategoryPicker.SelectedItem = item.Category;
            QuantityEntry.Text = item.StockLevel.ToString();
            PriceEntry.Text = item.UnitPrice.ToString();

            if (item.PromoId.HasValue && PromotionPicker.ItemsSource is List<Promotion> promos)
            {
                PromotionPicker.SelectedItem = promos.FirstOrDefault(p => p.PromoID == item.PromoId);
            }
            else
            {
                PromotionPicker.SelectedItem = null;
            }

            ItemModal.IsVisible = true;
        }
    }

    private async void OnDeleteItemClicked(object sender, EventArgs e)
    {
        if (sender is Label label && label.GestureRecognizers.Count > 0 && label.GestureRecognizers[0] is TapGestureRecognizer tap && tap.CommandParameter is Product item)
        {
            bool confirm = await DisplayAlertAsync("Delete", "Delete this product?", "Yes", "No");
            if (confirm)
            {
                await _dbService.ExecuteNonQueryAsync("DELETE FROM Products WHERE Id = @Id", new Dictionary<string, object> { { "@Id", item.Id } });
                LoadInventory();
            }
        }
    }

    private async void OnSaveItemClicked(object sender, EventArgs e)
    {
        string name = ItemNameEntry.Text;
        string sku = SKUEntry.Text;
        string category = CategoryPicker.SelectedItem?.ToString() ?? "General";
        int stock = int.TryParse(QuantityEntry.Text, out int s) ? s : 0;
        decimal price = decimal.TryParse(PriceEntry.Text, out decimal p) ? p : 0m;
        int? promoId = (PromotionPicker.SelectedItem as Promotion)?.PromoID;
        
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(sku))
        {
            await DisplayAlertAsync("Validation", "Name and SKU are required.", "OK");
            return;
        }

        if (_editingItemId == -1)
        {
            await Task.Run(() => _dbService.AddInventory(name, sku, category, stock, "pcs", price, promoId.HasValue ? "On Sale" : "In Stock", _selectedImagePath, promoId));
        }
        else
        {
            await Task.Run(() => _dbService.UpdateInventory(_editingItemId, name, sku, category, stock, "pcs", price, promoId.HasValue ? "On Sale" : "In Stock", _selectedImagePath, promoId));
        }

        ItemModal.IsVisible = false;
        LoadInventory();
    }

    private void OnCloseModalClicked(object sender, EventArgs e) => ItemModal.IsVisible = false;
}
