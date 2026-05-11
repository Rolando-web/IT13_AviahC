using System.Collections.ObjectModel;
using System.Data;
using IT13_AviahC.Models;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin;

public partial class AdminProductionPage : ContentPage
{
    private readonly DatabaseService _dbService;
    public ObservableCollection<ProductionBatch> ProductionBatches { get; } = new();
    public List<Product> AvailableProducts { get; set; } = new();

    public AdminProductionPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        ProductionCollection.ItemsSource = ProductionBatches;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            // Load Batches
            DataTable dt = await _dbService.GetAllProductionBatchesAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProductionBatches.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    ProductionBatches.Add(new ProductionBatch
                    {
                        BatchID = row["BatchID"].ToString() ?? "",
                        ProductID = Convert.ToInt32(row["ProductID"]),
                        ProductName = row["ProductName"].ToString() ?? "",
                        TargetQuantity = Convert.ToInt32(row["TargetQuantity"]),
                        ProducedQuantity = Convert.ToInt32(row["ProducedQuantity"]),
                        Defects = Convert.ToInt32(row["Defects"]),
                        Status = row["Status"].ToString() ?? "Planned",
                        StartDate = Convert.ToDateTime(row["StartDate"])
                    });
                }

                // Update Goals Summary (Dummy calculation for now based on actual data)
                int totalTarget = ProductionBatches.Sum(b => b.TargetQuantity);
                int totalProduced = ProductionBatches.Sum(b => b.ProducedQuantity);
                GoalText.Text = $"{totalProduced:N0} / {totalTarget:N0}";
                GoalPercent.Text = totalTarget > 0 ? $"{(int)((double)totalProduced / totalTarget * 100)}%" : "0%";
                GoalProgress.WidthRequest = totalTarget > 0 ? Math.Min(180, (double)totalProduced / totalTarget * 180) : 0;
            });

            // Load Products for the Picker
            DataTable prodDt = await _dbService.GetAllInventoryAsync();
            var products = new List<Product>();
            foreach (DataRow row in prodDt.Rows)
            {
                products.Add(new Product { Id = Convert.ToInt32(row["ProductID"]), ProductName = row["ProductName"].ToString() });
            }
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BatchPicker.ItemsSource = products;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Production Load Error: {ex.Message}");
        }
    }

    private async void OnSaveProductionRecord(object sender, EventArgs e)
    {
        try
        {
            if (BatchPicker.SelectedItem is not ProductionBatch selectedBatch)
            {
                await DisplayAlert("Validation", "Please select an active batch.", "OK");
                return;
            }

            if (!int.TryParse(QuantityEntry.Text, out int qty) || qty <= 0)
            {
                await DisplayAlert("Validation", "Enter a valid quantity produced.", "OK");
                return;
            }

            int defects = int.TryParse(DefectsEntry.Text, out int d) ? d : 0;

            int result = await _dbService.UpdateProductionOutputAsync(selectedBatch.BatchID, qty, defects);
            if (result > 0)
            {
                await DisplayAlert("Success", $"Recorded {qty} units for {selectedBatch.BatchID}.", "OK");
                QuantityEntry.Text = "";
                DefectsEntry.Text = "";
                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnNewBatchClicked(object sender, EventArgs e)
    {
        string batchId = "PB-" + new Random().Next(1000, 9999);
        // In a real app, you'd show a modal here to pick product and qty
        // For now, let's just use the "Daily Record" fields as a proxy or simple alert
        await DisplayAlert("New Batch", $"Use the 'Daily Record' section to manage production for {batchId}.", "OK");
    }
}
