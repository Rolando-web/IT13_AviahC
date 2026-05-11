using System.Data;
using System.Collections.ObjectModel;
using IT13_AviahC.Services;
using Microsoft.Maui.Controls;

namespace IT13_AviahC.Views.Admin;

public class WarehouseDisplayItem
{
    public string MaterialID { get; set; } = string.Empty;
    public string SKU => MaterialID; // For backward compatibility with UI bindings
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int StockLevel { get; set; }
    public string Unit { get; set; } = "pcs";
    public string StatusText { get; set; } = "In Stock";
    public string ImageUrl { get; set; } = string.Empty;

    public string StatusColor => StatusText switch
    {
        "In Stock" => "#DCFCE7",
        "Low Stock" => "#FEF3C7",
        "Critical" => "#FEE2E2",
        _ => "#F1F5F9"
    };

    public string StatusTextColor => StatusText switch
    {
        "In Stock" => "#15803D",
        "Low Stock" => "#D97706",
        "Critical" => "#DC2626",
        _ => "#64748B"
    };
}

public partial class AdminInventoryPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private string? _editingMaterialId;
    private string _selectedImagePath = string.Empty;
    public ObservableCollection<WarehouseDisplayItem> InventoryItems { get; } = new();

    public AdminInventoryPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        InventoryCollection.ItemsSource = InventoryItems;
        ItemModalView.CloseRequested += OnCloseModalRequested;
        ItemModalView.SaveRequested += OnSaveItemRequested;
        ItemModalView.UploadImageRequested += OnUploadImageRequested;
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
            DataTable dt = await _dbService.GetAllWarehouseItemsAsync();
            InventoryItems.Clear();

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string name = row["ItemName"]?.ToString() ?? "Unknown";
                    string img = row["ImageUrl"]?.ToString() ?? "";
                    
                    InventoryItems.Add(new WarehouseDisplayItem
                    {
                        MaterialID = row["MaterialID"]?.ToString() ?? "N/A",
                        ItemName = name,
                        Category = row["Category"]?.ToString() ?? "General",
                        StockLevel = row["Quantity"] != DBNull.Value ? Convert.ToInt32(row["Quantity"]) : 0,
                        Unit = row["Unit"]?.ToString() ?? "pcs",
                        StatusText = row["Status"]?.ToString() ?? "In Stock",
                        ImageUrl = string.IsNullOrEmpty(img) ? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background=random&size=128" : img
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "Failed to load inventory: " + ex.Message, "OK");
        }
    }

    private void OnAddItemClicked(object sender, EventArgs e)
    {
        _editingMaterialId = null;
        _selectedImagePath = string.Empty;
        ItemModalView.ShowForNew();
        ItemModalView.IsVisible = true;
    }

    private void OnEditItemClicked(object sender, EventArgs e)
    {
        if (sender is Label label && label.GestureRecognizers.Count > 0 && label.GestureRecognizers[0] is TapGestureRecognizer tap && tap.CommandParameter is WarehouseDisplayItem item)
        {
            _editingMaterialId = item.MaterialID;
            _selectedImagePath = item.ImageUrl;
            ItemModalView.ShowForEdit(item.ItemName, item.MaterialID, item.Category, item.StockLevel, item.Unit, item.StatusText, _selectedImagePath);
            ItemModalView.IsVisible = true;
        }
    }

    private async void OnDeleteItemClicked(object sender, EventArgs e)
    {
        if (sender is Label lbl && lbl.GestureRecognizers.Count > 0 && lbl.GestureRecognizers[0] is TapGestureRecognizer tap && tap.CommandParameter is WarehouseDisplayItem item)
        {
            bool confirm = await DisplayAlertAsync("Delete", $"Remove '{item.ItemName}' ({item.MaterialID})?", "Yes", "No");
            if (confirm)
            {
                int result = await _dbService.DeleteWarehouseItemAsync(item.MaterialID);
                if (result > 0)
                {
                    await DisplayAlertAsync("Success", "Item removed.", "OK");
                    LoadInventory();
                }
            }
        }
    }

    private void OnCloseModalRequested(object? sender, EventArgs e) => ItemModalView.IsVisible = false;

    private async void OnSaveItemRequested(object? sender, EventArgs e)
    {
        string sku = ItemModalView.SKU ?? string.Empty;
        string name = ItemModalView.ItemName ?? string.Empty;
        string category = ItemModalView.Category;
        int qty = ItemModalView.Quantity;
        string unit = ItemModalView.Unit;
        string status = ItemModalView.Status;

        int result;
        if (string.IsNullOrEmpty(_editingMaterialId))
        {
            result = _dbService.AddWarehouseItem(sku, name, category, qty, unit, status, _selectedImagePath);
        }
        else
        {
            result = _dbService.UpdateWarehouseItem(_editingMaterialId, sku, name, category, qty, unit, status, _selectedImagePath);
        }

        if (result > 0)
        {
            ItemModalView.IsVisible = false;
            LoadInventory();
        }
        else
        {
            await DisplayAlertAsync("Error", "Could not save item.", "OK");
        }
    }

    private async void OnUploadImageRequested(object? sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Select Image", FileTypes = FilePickerFileType.Images });
        if (result != null)
        {
            _selectedImagePath = result.FullPath;
            ItemModalView.UpdateImagePreview(_selectedImagePath);
        }
    }
}
