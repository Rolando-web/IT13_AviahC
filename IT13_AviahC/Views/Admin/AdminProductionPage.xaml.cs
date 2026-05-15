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

        ProductionModalView.CloseRequested += (s, e) => ProductionModalView.IsVisible = false;
        ProductionModalView.StartProductionRequested += OnModalStartProduction;
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
            // Load Finished Products (Catalog)
            DataTable finishedDt = await _dbService.GetFinishedProductsAsync();
            var catalogItems = new List<object>();
            
            foreach (DataRow row in finishedDt.Rows)
            {
                int pid = Convert.ToInt32(row["ProductID"]);
                DataTable materialsDt = await _dbService.GetProductMaterialsAsync(pid);
                
                var materialList = new List<string>();
                foreach (DataRow mRow in materialsDt.Rows)
                {
                    materialList.Add($"{mRow["QuantityRequired"]} {mRow["Unit"]} {mRow["MaterialName"]}");
                }

                catalogItems.Add(new 
                {
                    ProductID = pid,
                    ProductName = row["ProductName"]?.ToString(),
                    Category = row["Category"]?.ToString(),
                    ImageUrl = row["ImageUrl"]?.ToString() ?? "dress.png",
                    Material1 = materialList.Count > 0 ? materialList[0] : "None specified",
                    Material2 = materialList.Count > 1 ? materialList[1] : "N/A"
                });
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                FinishedProductsCollection.ItemsSource = catalogItems;
            });

            // Load Active Batches for the List
            DataTable dt = await _dbService.GetAllProductionBatchesAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var activeBatches = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    int target = Convert.ToInt32(row["TargetQuantity"]);
                    int produced = Convert.ToInt32(row["ProducedQuantity"]);
                    double progress = target > 0 ? (double)produced / target : 0;
                    if (progress > 1.0) progress = 1.0; // Cap at 100% so it doesn't overlap

                    string status = row["Status"]?.ToString() ?? "In Progress";
                    DateTime started = Convert.ToDateTime(row["StartDate"] != DBNull.Value ? row["StartDate"] : DateTime.Now);
                    
                    bool isCompleted = status == "Completed" || progress >= 1.0;

                    activeBatches.Add(new 
                    {
                        BatchID = row["BatchID"].ToString() ?? "",
                        ProductName = row["ProductName"].ToString() ?? "",
                        TargetText = $"Target Quantity: {target} pcs",
                        TimeStatus = isCompleted ? "Completed recently" : $"Started {Math.Max(1, (DateTime.Now - started).Days)} days ago",
                        StatusIcon = isCompleted ? "✔️" : "✂️",
                        StatusBoxColor = isCompleted ? Color.FromArgb("#DCFCE7") : Color.FromArgb("#F3F0FA"),
                        StatusIconColor = isCompleted ? Color.FromArgb("#16A34A") : Color.FromArgb("#624890"),
                        ProgressText = $"{produced} / {target} pcs",
                        ProgressWidth = Math.Min(220, progress * 220),
                        ProgressColor = isCompleted ? Color.FromArgb("#22C55E") : Color.FromArgb("#624890"),
                        ActionButtonText = isCompleted ? "Approve Batch" : "Update Progress",
                        IsActive = !isCompleted
                    });
                }
                ProductionCollection.ItemsSource = activeBatches;
            });

            // Update Goals Summary
            int totalTarget = 0;
            int totalProduced = 0;
            int totalDefects = 0;

            foreach (DataRow row in dt.Rows)
            {
                totalTarget += Convert.ToInt32(row["TargetQuantity"]);
                totalProduced += Convert.ToInt32(row["ProducedQuantity"]);
                if (dt.Columns.Contains("Defects") && row["Defects"] != DBNull.Value)
                {
                    totalDefects += Convert.ToInt32(row["Defects"]);
                }
            }

            if (totalTarget == 0) totalTarget = 500; // default baseline

            double weeklyProgress = (double)totalProduced / totalTarget;
            if (weeklyProgress > 1.0) weeklyProgress = 1.0;

            double qcPassRate = totalProduced > 0 ? (double)(totalProduced - totalDefects) / totalProduced : 1.0;
            double materialUtil = 0.85; // Placeholder since no raw material loss tracking exists yet

            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Update Active Products Summary
                var activeProductNames = dt.Rows.Cast<DataRow>()
                    .Where(r => r["Status"]?.ToString() != "Completed")
                    .Select(r => r["ProductName"].ToString())
                    .Distinct()
                    .ToList();
                
                ActiveProductsSummaryLabel.Text = activeProductNames.Any() 
                    ? $"Currently Producing: {string.Join(", ", activeProductNames)}"
                    : "Currently Producing: None (All batches completed)";

                // Update Weekly Quota
                WeeklyGoalLabel.Text = $"{totalProduced} / {totalTarget} pcs";
                WeeklyGoalPercentLabel.Text = $"{(int)(weeklyProgress * 100)}% Completed";
                WeeklyGoalBar.WidthRequest = weeklyProgress * 200;

                // Update QC
                QcPassRateLabel.Text = $"{qcPassRate:P1}";
                QcPassRateBar.WidthRequest = qcPassRate * 260;
                QcStatusLabel.Text = qcPassRate >= 0.95 ? "Excellent" : "Needs Improvement";
                QcStatusLabel.TextColor = qcPassRate >= 0.95 ? Color.FromArgb("#624890") : Color.FromArgb("#EF4444");

                // Update Material
                MaterialUtilLabel.Text = $"{materialUtil:P0} Efficiency";
                MaterialUtilBar.WidthRequest = materialUtil * 230;
                MaterialStatusLabel.Text = materialUtil >= 0.80 ? "On Track" : "Low Efficiency";
                MaterialStatusLabel.TextColor = materialUtil >= 0.80 ? Color.FromArgb("#F59E0B") : Color.FromArgb("#EF4444");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Production Load Error: {ex.Message}");
        }
    }

    private async void OnMakeProductClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is object item)
        {
            dynamic dItem = item;
            int productId = dItem.ProductID;
            string productName = dItem.ProductName;

            DataTable materialsDt = await _dbService.GetProductMaterialsAsync(productId);
            
            ProductionModalView.LoadProductDetails(productId, productName, materialsDt);
            ProductionModalView.IsVisible = true;
        }
    }

    private async void OnModalStartProduction(object? sender, int qty)
    {
        ProductionModalView.IsVisible = false;
        int productId = ProductionModalView.ProductId;
        string productName = ProductionModalView.ProductName;
        
        string batchId = "BATCH-" + new Random().Next(100, 999);
        await _dbService.CreateProductionBatchAsync(batchId, productId, qty);
        
        // Deduct used raw materials
        DataTable materialsDt = await _dbService.GetProductMaterialsAsync(productId);
        foreach (DataRow row in materialsDt.Rows)
        {
            string matId = row["MaterialID"]?.ToString() ?? "";
            double qtyReq = Convert.ToDouble(row["QuantityRequired"]);
            double deduct = qtyReq * qty;
            
            string query = "UPDATE RawMaterials SET Quantity = Quantity - @deduct WHERE MaterialID = @matId";
            await _dbService.ExecuteNonQueryAsync(query, new Dictionary<string, object> { {"@deduct", deduct}, {"@matId", matId} });
        }

        await DisplayAlertAsync("Started", $"Production for {productName} has started. It will take approximately 1 week to complete.", "OK");
        await LoadDataAsync();
    }

    private async void OnUpdateBatchClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is object batchObj)
        {
            dynamic batch = batchObj;
            string batchId = batch.BatchID;

            string qtyStr = await DisplayPromptAsync("Update Progress", $"Enter units produced for {batchId}:", "Save", "Cancel", "10", -1, Keyboard.Numeric);
            if (int.TryParse(qtyStr, out int qty) && qty > 0)
            {
                int result = await _dbService.UpdateProductionOutputAsync(batchId, qty, 0);
                if (result > 0)
                {
                    await DisplayAlertAsync("Success", $"Recorded {qty} units. Sent to Warehouse.", "OK");
                    await LoadDataAsync();
                }
            }
        }
    }

    private async void OnNewBatchClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Manual Batch", "Please use the 'Make' button on a product card below to start a scheduled 1-week production batch.", "OK");
    }

}
