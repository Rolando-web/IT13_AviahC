using Microsoft.Maui.Controls;
using System.Data;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin;

public partial class AdminInventoryPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public AdminInventoryPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
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
            DataTable dt = _dbService.GetAllInventory();
            var inventory = new List<object>();

            foreach (DataRow row in dt.Rows)
            {
                inventory.Add(new
                {
                    ItemName = row["ItemName"].ToString(),
                    Category = row["Category"].ToString(),
                    StockLevel = Convert.ToInt32(row["StockLevel"]),
                    Unit = row["Unit"].ToString(),
                    UnitPrice = Convert.ToDecimal(row["UnitPrice"])
                });
            }

            InventoryCollection.ItemsSource = inventory;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "Failed to load inventory: " + ex.Message, "OK");
        }
    }
}
