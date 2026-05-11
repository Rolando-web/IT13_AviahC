using System.Data;
using System.Collections.ObjectModel;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Supplier;

public class WarehouseRowItem
{
    public string MaterialID { get; set; } = string.Empty;
    public string ItemName   { get; set; } = string.Empty;
    public string Category   { get; set; } = string.Empty;
    public int    Quantity   { get; set; }
    public string Unit       { get; set; } = "pcs";
    public string Status     { get; set; } = "In Stock";

    public string StatusBg => Status switch
    {
        "In Stock"  => "#DCFCE7",
        "Low Stock" => "#FEF3C7",
        "Critical"  => "#FEE2E2",
        _           => "#F1F5F9"
    };
    public string StatusFg => Status switch
    {
        "In Stock"  => "#15803D",
        "Low Stock" => "#D97706",
        "Critical"  => "#DC2626",
        _           => "#64748B"
    };
}

public partial class SupplierInventoryPage : ContentPage
{
    private readonly DatabaseService _db;
    private bool   _isEditing      = false;
    private string _editingId      = string.Empty;
    public  ObservableCollection<WarehouseRowItem> Items { get; } = new();

    public SupplierInventoryPage()
    {
        InitializeComponent();
        _db = new DatabaseService();
        WarehouseCollection.ItemsSource = Items;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadItemsAsync();
    }

    // ─── LOAD ────────────────────────────────────────────────────────────────
    private async Task LoadItemsAsync()
    {
        try
        {
            DataTable dt = await _db.GetWarehouseForSupplierAsync();
            var list = new List<WarehouseRowItem>();
            int lowStock = 0, critical = 0;

            foreach (DataRow row in dt.Rows)
            {
                string status = row["Status"]?.ToString() ?? "In Stock";
                if (status == "Low Stock") lowStock++;
                if (status == "Critical")  critical++;

                list.Add(new WarehouseRowItem
                {
                    MaterialID = row["MaterialID"]?.ToString() ?? string.Empty,
                    ItemName   = row["ItemName"]?.ToString()   ?? "Unknown",
                    Category   = row["Category"]?.ToString()   ?? "General",
                    Quantity   = row["Quantity"] != DBNull.Value ? Convert.ToInt32(row["Quantity"]) : 0,
                    Unit       = row["Unit"]?.ToString()       ?? "pcs",
                    Status     = status
                });
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Items.Clear();
                foreach (var item in list) Items.Add(item);
                TotalItemsLabel.Text = list.Count.ToString();
                LowStockLabel.Text   = lowStock.ToString();
                CriticalLabel.Text   = critical.ToString();
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "Failed to load warehouse: " + ex.Message, "OK");
        }
    }

    private void OnRefreshClicked(object sender, EventArgs e) => _ = LoadItemsAsync();

    // ─── OPEN ADD MODAL ──────────────────────────────────────────────────────
    private void OnAddItemClicked(object sender, EventArgs e)
    {
        _isEditing      = false;
        _editingId      = string.Empty;
        ModalTitle.Text = "Record Material Delivery";
        ClearModalFields();
        ModalOverlay.IsVisible = true;
    }

    // ─── OPEN EDIT MODAL ─────────────────────────────────────────────────────
    private void OnEditItemClicked(object sender, EventArgs e)
    {
        if (sender is Label lbl &&
            lbl.GestureRecognizers.Count > 0 &&
            lbl.GestureRecognizers[0] is TapGestureRecognizer tap &&
            tap.CommandParameter is WarehouseRowItem item)
        {
            _isEditing      = true;
            _editingId      = item.MaterialID;
            ModalTitle.Text = "Edit Material";
            SkuEntry.Text   = item.MaterialID;
            NameEntry.Text  = item.ItemName;
            CategoryPicker.SelectedItem = item.Category;
            QuantityEntry.Text = item.Quantity.ToString();
            UnitPicker.SelectedItem = item.Unit;
            ModalOverlay.IsVisible = true;
        }
    }

    // ─── DELETE ──────────────────────────────────────────────────────────────
    private async void OnDeleteItemClicked(object sender, EventArgs e)
    {
        if (sender is Label lbl &&
            lbl.GestureRecognizers.Count > 0 &&
            lbl.GestureRecognizers[0] is TapGestureRecognizer tap &&
            tap.CommandParameter is WarehouseRowItem item)
        {
            bool confirm = await DisplayAlertAsync(
                "Delete", $"Remove '{item.ItemName}' ({item.MaterialID}) from the warehouse?", "Yes", "No");
            if (!confirm) return;

            int result = await _db.SupplierDeleteMaterialAsync(item.MaterialID);
            if (result > 0)
            {
                await DisplayAlertAsync("Deleted", $"'{item.ItemName}' has been removed.", "OK");
                await LoadItemsAsync();
            }
            else
            {
                await DisplayAlertAsync("Error", "Failed to delete item.", "OK");
            }
        }
    }

    private void OnCloseModalClicked(object sender, EventArgs e)
    {
        ModalOverlay.IsVisible = false;
        _isEditing = false;
        _editingId = string.Empty;
    }

    // ─── SAVE (ADD or EDIT) ──────────────────────────────────────────────────
    private async void OnSaveItemClicked(object sender, EventArgs e)
    {
        string sku      = SkuEntry.Text?.Trim()   ?? string.Empty;
        string name     = NameEntry.Text?.Trim()  ?? string.Empty;
        string category = CategoryPicker.SelectedItem?.ToString() ?? "Other";
        string unit     = UnitPicker.SelectedItem?.ToString()     ?? "pcs";

        if (!int.TryParse(QuantityEntry.Text, out int qty) || qty <= 0)
        {
            await DisplayAlertAsync("Validation", "Please enter a valid quantity greater than 0.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlertAsync("Validation", "Material ID and Item Name are required.", "OK");
            return;
        }

        int result;
        if (_isEditing)
        {
            result = await _db.SupplierUpdateMaterialAsync(_editingId, name, category, qty, unit);
        }
        else
        {
            string supplierEmail = UserSession.UserEmail ?? "supplier@aviah.com";
            result = await _db.SupplierAddMaterialDeliveryAsync(sku, name, category, qty, unit, supplierEmail);
        }

        if (result > 0)
        {
            await DisplayAlertAsync("Success",
                _isEditing
                    ? $"'{name}' has been updated successfully."
                    : $"Supply of {qty} {unit} of '{name}' recorded successfully.", "OK");
            ModalOverlay.IsVisible = false;
            await LoadItemsAsync();
        }
        else
        {
            await DisplayAlertAsync("Error", "Operation failed. Please try again.", "OK");
        }
    }

    private void ClearModalFields()
    {
        SkuEntry.Text           = string.Empty;
        NameEntry.Text          = string.Empty;
        CategoryPicker.SelectedIndex = -1;
        QuantityEntry.Text      = string.Empty;
        UnitPicker.SelectedIndex = -1;
    }
}
