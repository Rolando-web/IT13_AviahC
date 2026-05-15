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
    private bool _isWarehouseView = false;

    public Color BoutiqueTabColor => !_isWarehouseView ? Color.FromArgb("#624890") : Color.FromArgb("#94A3B8");
    public Color WarehouseTabColor => _isWarehouseView ? Color.FromArgb("#624890") : Color.FromArgb("#94A3B8");
    public Color BoutiqueTabUnderline => !_isWarehouseView ? Color.FromArgb("#624890") : Colors.Transparent;
    public Color WarehouseTabUnderline => _isWarehouseView ? Color.FromArgb("#624890") : Colors.Transparent;

    public StaffInventoryPage()
    {
        InitializeComponent();
        BindingContext = this;
        _dbService = new DatabaseService();
        InventoryCollection.ItemsSource = InventoryItems;
        ItemModalView.CloseRequested += OnCloseModalRequested;
        ItemModalView.SaveRequested += OnSaveItemRequested;
        ItemModalView.UploadImageRequested += OnUploadImageRequested;
        LoadPromotions();
    }

    private void OnBoutiqueTabClicked(object sender, EventArgs e)
    {
        _isWarehouseView = false;
        OnPropertyChanged(nameof(BoutiqueTabColor));
        OnPropertyChanged(nameof(WarehouseTabColor));
        OnPropertyChanged(nameof(BoutiqueTabUnderline));
        OnPropertyChanged(nameof(WarehouseTabUnderline));
        LoadInventory();
    }

    private void OnWarehouseTabClicked(object sender, EventArgs e)
    {
        _isWarehouseView = true;
        OnPropertyChanged(nameof(BoutiqueTabColor));
        OnPropertyChanged(nameof(WarehouseTabColor));
        OnPropertyChanged(nameof(BoutiqueTabUnderline));
        OnPropertyChanged(nameof(WarehouseTabUnderline));
        LoadInventory();
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
            
            MainThread.BeginInvokeOnMainThread(() => {
                InventoryItems.Clear();
                int warehouseItemCount = 0;

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        int boutiqueStock = row["StockQuantity"] != DBNull.Value ? Convert.ToInt32(row["StockQuantity"]) : 0;
                        int warehouseStock = row.Table.Columns.Contains("WarehouseStock") && row["WarehouseStock"] != DBNull.Value ? Convert.ToInt32(row["WarehouseStock"]) : 0;
                        
                        if (warehouseStock > 0) warehouseItemCount++;

                        // Filter based on view
                        if (_isWarehouseView && warehouseStock <= 0) continue;
                        
                        string img = row["ImageUrl"]?.ToString() ?? "";
                        string name = row["ProductName"]?.ToString() ?? "Unknown";
                        string statusText = "In Stock";
                        string statusColor = "#DCFCE7";
                        string statusTextColor = "#16A34A";

                        int currentStock = _isWarehouseView ? warehouseStock : boutiqueStock;

                        if (currentStock == 0)
                        {
                            statusText = "Out of Stock";
                            statusColor = "#FEE2E2";
                            statusTextColor = "#EF4444";
                        }
                        else if (currentStock < 10)
                        {
                            statusText = "Low Stock";
                            statusColor = "#FEF3C7";
                            statusTextColor = "#D97706";
                        }

                        if (_isWarehouseView)
                        {
                            statusText = "Ready: " + warehouseStock + " pcs";
                            statusColor = "#E0F2FE"; 
                            statusTextColor = "#0369A1";
                        }

                        InventoryItems.Add(new Product
                        {
                            Id = Convert.ToInt32(row["ProductID"] ?? 0),
                            ProductName = name,
                            SKU = row.Table.Columns.Contains("SKU") ? row["SKU"]?.ToString() : "N/A",
                            Category = row["Category"]?.ToString() ?? "General",
                            StockLevel = currentStock,
                            WarehouseStock = warehouseStock,
                            UnitPrice = row["Price"] != DBNull.Value ? Convert.ToDecimal(row["Price"]) : 0m,
                            StatusText = statusText,
                            StatusColor = statusColor,
                            StatusTextColor = statusTextColor,
                            ImageUrl = string.IsNullOrEmpty(img) ? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background=random&size=128" : img,
                            IsWarehouseView = _isWarehouseView,
                            IsBoutiqueView = !_isWarehouseView
                        });
                    }
                }

                WarehouseBadge.IsVisible = warehouseItemCount > 0;
                WarehouseCountLabel.Text = warehouseItemCount.ToString();
                EmptyStateView.IsVisible = InventoryItems.Count == 0;
                InventoryCollection.IsVisible = InventoryItems.Count > 0;
            });
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
            _selectedImagePath = item.ImageUrl ?? string.Empty;
            ItemModalView.ShowForEdit(item, _promotions);
            ItemModalView.UpdateImagePreview(_selectedImagePath);

            ItemModalView.IsVisible = true;
        }
    }

    private async void OnStockInClicked(object sender, EventArgs e)
    {
        if (sender is Label label && label.GestureRecognizers.Count > 0 && label.GestureRecognizers[0] is TapGestureRecognizer tap && tap.CommandParameter is Product item)
        {
            string result = await DisplayPromptAsync("Stock-in", $"Moving {item.ProductName} to Boutique.\nAvailable in Warehouse: {item.WarehouseStock}", "Transfer", "Cancel", "1", -1, Keyboard.Numeric);
            
            if (int.TryParse(result, out int qty))
            {
                if (qty > item.WarehouseStock)
                {
                    await DisplayAlertAsync("Exception", "Error: Stock-in quantity exceeds available warehouse stock!", "OK");
                    return;
                }

                if (qty <= 0) return;

                // Move stock
                string query = "UPDATE Products SET StockQuantity = StockQuantity + @Qty, WarehouseStock = WarehouseStock - @Qty WHERE ProductID = @Id";
                await _dbService.ExecuteNonQueryAsync(query, new Dictionary<string, object> { { "@Qty", qty }, { "@Id", item.Id } });
                
                await DisplayAlertAsync("Success", $"Successfully stocked {qty} units into the boutique.", "OK");
                LoadInventory();
            }
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
        int warehouseStock = ItemModalView.WarehouseStock;
        decimal price = ItemModalView.UnitPrice;
        int? promoId = ItemModalView.PromoId;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(sku))
        {
            await DisplayAlertAsync("Validation", "Name and SKU are required.", "OK");
            return;
        }

        if (_editingItemId == -1)
        {
            await Task.Run(() => _dbService.AddInventory(name, sku, category, stock, warehouseStock, "pcs", price, promoId.HasValue ? "On Sale" : "In Stock", _selectedImagePath, promoId));
        }
        else
        {
            await Task.Run(() => _dbService.UpdateInventory(_editingItemId, name, sku, category, stock, warehouseStock, "pcs", price, promoId.HasValue ? "On Sale" : "In Stock", _selectedImagePath, promoId));
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
