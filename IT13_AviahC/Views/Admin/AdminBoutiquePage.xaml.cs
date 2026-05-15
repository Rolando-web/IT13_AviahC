using System.Data;
using System.Collections.ObjectModel;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin;

public class BoutiqueDisplayItem
{
    public int ProductID { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}

public partial class AdminBoutiquePage : ContentPage
{
    private readonly DatabaseService _dbService;
    public ObservableCollection<BoutiqueDisplayItem> BoutiqueItems { get; } = new();
    public bool IsCrudEnabled => UserSession.CurrentTier != "Basic";

    public AdminBoutiquePage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        BoutiqueCollection.ItemsSource = BoutiqueItems;
        ApplyTierLocking();
    }

    private void ApplyTierLocking()
    {
        bool allowed = IsCrudEnabled;
        AddItemButton.IsVisible = allowed;
        ActionsColumnHeader.IsVisible = allowed;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadBoutiqueInventory();
    }

    private async void LoadBoutiqueInventory()
    {
        try
        {
            DataTable dt = await _dbService.GetAllInventoryAsync();
            BoutiqueItems.Clear();

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string name = row["ProductName"]?.ToString() ?? "Unknown";
                    string img = row["ImageUrl"]?.ToString() ?? "";
                    
                    BoutiqueItems.Add(new BoutiqueDisplayItem
                    {
                        ProductID = row["ProductID"] != DBNull.Value ? Convert.ToInt32(row["ProductID"]) : 0,
                        ProductName = name,
                        SKU = row["SKU"]?.ToString() ?? "N/A",
                        Category = row["Category"]?.ToString() ?? "General",
                        StockQuantity = row["StockQuantity"] != DBNull.Value ? Convert.ToInt32(row["StockQuantity"]) : 0,
                        Price = row["Price"] != DBNull.Value ? Convert.ToDecimal(row["Price"]) : 0,
                        ImageUrl = string.IsNullOrEmpty(img) ? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background=random&size=128" : img
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "Failed to load boutique inventory: " + ex.Message, "OK");
        }
    }
}
