using System.Data;
using System.Collections.ObjectModel;
using IT13_AviahC.Services;
using IT13_AviahC.Models;
using Microsoft.Maui.Controls;

namespace IT13_AviahC.Views.Admin;

public partial class AdminInventoryPage : ContentPage
{
    private readonly DatabaseService _dbService;
    public ObservableCollection<Product> InventoryItems { get; set; } = new ObservableCollection<Product>();

    public AdminInventoryPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        
        if (InventoryCollection != null)
        {
            InventoryCollection.ItemsSource = InventoryItems;
        }
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
            if (_dbService == null) return;

            DataTable dt = await _dbService.GetAllInventoryAsync();
            if (dt == null || dt.Rows == null) return;

            InventoryItems.Clear();
            int index = 1;

            foreach (DataRow row in dt.Rows)
            {
                if (row == null) continue;

                int id = 0;
                if (dt.Columns.Contains("Id") && row["Id"] != DBNull.Value)
                    id = Convert.ToInt32(row["Id"]);

                int stock = 0;
                if (dt.Columns.Contains("StockLevel") && row["StockLevel"] != DBNull.Value)
                    stock = Convert.ToInt32(row["StockLevel"]);

                string statusText = "In Stock";
                if (dt.Columns.Contains("Status"))
                    statusText = row["Status"]?.ToString() ?? "In Stock";

                string statusColor = "#DCFCE7";
                string statusTextColor = "#15803D";

                if (statusText == "Critical" || stock <= 10)
                {
                    statusText = "Critical";
                    statusColor = "#FEE2E2";
                    statusTextColor = "#DC2626";
                }
                else if (statusText == "Low Stock" || stock <= 100)
                {
                    statusText = "Low Stock";
                    statusColor = "#FEF3C7";
                    statusTextColor = "#D97706";
                }

                string itemName = "Unknown Item";
                if (dt.Columns.Contains("ProductName"))
                    itemName = row["ProductName"]?.ToString() ?? "Unknown Item";
                else if (dt.Columns.Contains("ItemName"))
                    itemName = row["ItemName"]?.ToString() ?? "Unknown Item";

                string imageUrl = "";
                if (dt.Columns.Contains("ImageUrl"))
                    imageUrl = row["ImageUrl"]?.ToString() ?? "";
                
                if (string.IsNullOrEmpty(imageUrl))
                {
                    imageUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(itemName)}&background=random&size=128";
                }

                string sku = $"RM-{index:D3}";
                if (dt.Columns.Contains("SKU"))
                    sku = row["SKU"]?.ToString() ?? $"RM-{index:D3}";

                string category = "General";
                if (dt.Columns.Contains("Category"))
                    category = row["Category"]?.ToString() ?? "General";

                string unit = "pcs";
                if (dt.Columns.Contains("Unit"))
                    unit = row["Unit"]?.ToString() ?? "pcs";

                decimal price = 0m;
                if (dt.Columns.Contains("Price") && row["Price"] != DBNull.Value)
                    price = Convert.ToDecimal(row["Price"]);
                else if (dt.Columns.Contains("UnitPrice") && row["UnitPrice"] != DBNull.Value)
                    price = Convert.ToDecimal(row["UnitPrice"]);

                InventoryItems.Add(new Product
                {
                    Id = id,
                    ProductName = itemName,
                    SKU = sku,
                    Category = category,
                    StockLevel = stock,
                    Unit = unit,
                    UnitPrice = price,
                    StatusText = statusText,
                    StatusColor = statusColor,
                    StatusTextColor = statusTextColor,
                    ImageUrl = imageUrl
                });
                index++;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "Failed to load inventory: " + ex.Message, "OK");
        }
    }
}
