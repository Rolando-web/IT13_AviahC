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
    private string _selectedImagePath = string.Empty;
    private List<Promotion> _promotions = new();
    public ObservableCollection<Product> InventoryItems { get; set; } = new ObservableCollection<Product>();

    public StaffInventoryPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        InventoryCollection.ItemsSource = InventoryItems;
        ItemModalView.CloseRequested += OnCloseModalRequested;
        ItemModalView.SaveRequested += OnSaveItemRequested;
        ItemModalView.UploadImageRequested += OnUploadImageRequested;
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
            _promotions = promoList;
            ItemModalView.SetPromotions(_promotions);
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
                MainThread.BeginInvokeOnMainThread(() => {
                    EmptyStateView.IsVisible = false;
                    InventoryCollection.IsVisible = true;
                });

                foreach (DataRow row in dt.Rows)
                {
                    string img = row["ImageUrl"]?.ToString() ?? "";
                    var item = new Product
                    {
                        Id = Convert.ToInt32(row["ProductID"] ?? 0),
                        ProductName = row["ProductName"]?.ToString() ?? "Unknown",
                        SKU = row.Table.Columns.Contains("SKU") ? row["SKU"]?.ToString() : "N/A",
                        Category = row["Category"]?.ToString() ?? "General",
                        StockLevel = row["StockQuantity"] != DBNull.Value ? Convert.ToInt32(row["StockQuantity"]) : 0,
                        UnitPrice = row["Price"] != DBNull.Value ? Convert.ToDecimal(row["Price"]) : 0m,
                        StatusText = row.Table.Columns.Contains("Status") ? row["Status"]?.ToString() : "In Stock",
                        ImageUrl = string.IsNullOrEmpty(img) ? "package_icon.png" : img
                    };

                    MainThread.BeginInvokeOnMainThread(() => InventoryItems.Add(item));
                }
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => {
                    InventoryCollection.IsVisible = false;
                    EmptyStateView.IsVisible = true;
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Inventory Load Error: {ex.Message}");
        }
    }

    private async void OnUploadImageClicked(object sender, EventArgs e)
    {
        await PickImageAsync();
    }

    private void OnAddItemClicked(object sender, EventArgs e)
    {
        _editingItemId = -1;
        _selectedImagePath = string.Empty;
        ItemModalView.ShowForNew();
        ItemModalView.IsVisible = true;
    }

    private void OnEditItemClicked(object sender, EventArgs e)
    {
        if (sender is Label label && label.GestureRecognizers.Count > 0 && label.GestureRecognizers[0] is TapGestureRecognizer tap && tap.CommandParameter is Product item)
        {
            _editingItemId = item.Id;
            _selectedImagePath = item.ImageUrl;
            ItemModalView.ShowForEdit(item, _promotions);
            ItemModalView.UpdateImagePreview(_selectedImagePath);

            ItemModalView.IsVisible = true;
        }
    }

    private async void OnDeleteItemClicked(object sender, EventArgs e)
    {
        if (sender is Label label && label.GestureRecognizers.Count > 0 && label.GestureRecognizers[0] is TapGestureRecognizer tap && tap.CommandParameter is Product item)
        {
            bool confirm = await DisplayAlertAsync("Delete", "Delete this product?", "Yes", "No");
            if (confirm)
            {
                await _dbService.ExecuteNonQueryAsync("DELETE FROM Products WHERE ProductID = @Id", new Dictionary<string, object> { { "@Id", item.Id } });
                LoadInventory();
            }
        }
    }

    private void OnCloseModalRequested(object? sender, EventArgs e) => ItemModalView.IsVisible = false;

    private async void OnSaveItemRequested(object? sender, EventArgs e)
    {
        string name = ItemModalView.ProductName ?? string.Empty;
        string sku = ItemModalView.SKU ?? string.Empty;
        string category = ItemModalView.Category;
        int stock = ItemModalView.StockLevel;
        decimal price = ItemModalView.UnitPrice;
        int? promoId = ItemModalView.PromoId;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(sku))
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

        ItemModalView.IsVisible = false;
        LoadInventory();
    }

    private async void OnUploadImageRequested(object? sender, EventArgs e)
    {
        await PickImageAsync();
    }

    private async Task PickImageAsync()
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
                ItemModalView.UpdateImagePreview(_selectedImagePath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "Could not pick image: " + ex.Message, "OK");
        }
    }
}
