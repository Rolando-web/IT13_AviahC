using Microsoft.Maui.Controls;
using System.Data;

namespace IT13_AviahC.Views.Modals
{
    public partial class StartProductionModal : ContentView
    {
        public event EventHandler<int>? StartProductionRequested;
        public event EventHandler? CloseRequested;
        
        private int _maxPossible = 0;
        public int ProductId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        
        public StartProductionModal()
        {
            InitializeComponent();
        }

        public void LoadProductDetails(int productId, string productName, DataTable materialsDt)
        {
            ProductId = productId;
            ProductName = productName;
            ProductNameLabel.Text = productName;
            QuantityEntry.Text = string.Empty;
            ErrorLabel.IsVisible = false;
            StartButton.IsEnabled = false;

            MaterialsContainer.Children.Clear();
            
            _maxPossible = int.MaxValue;
            if (materialsDt.Rows.Count == 0) _maxPossible = 0;

            foreach (DataRow row in materialsDt.Rows)
            {
                string matName = row["MaterialName"]?.ToString() ?? "Unknown";
                double qtyReq = Convert.ToDouble(row["QuantityRequired"]);
                double stockQty = Convert.ToDouble(row["StockQuantity"]);
                string unit = row["Unit"]?.ToString() ?? "";

                int maxForThis = qtyReq > 0 ? (int)(stockQty / qtyReq) : 0;
                if (maxForThis < _maxPossible) _maxPossible = maxForThis;

                var lbl = new Label 
                { 
                    Text = $"• {matName}: Requires {qtyReq} {unit}/pc (In Stock: {stockQty} {unit})",
                    FontSize = 13,
                    TextColor = stockQty >= qtyReq ? Color.FromArgb("#463366") : Color.FromArgb("#EF4444")
                };
                MaterialsContainer.Children.Add(lbl);
            }

            if (_maxPossible < 0) _maxPossible = 0;
            MaxPossibleLabel.Text = $"{_maxPossible} pcs";
        }

        private void OnQuantityChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(e.NewTextValue, out int qty))
            {
                if (qty > _maxPossible)
                {
                    ErrorLabel.IsVisible = true;
                    StartButton.IsEnabled = false;
                }
                else if (qty > 0)
                {
                    ErrorLabel.IsVisible = false;
                    StartButton.IsEnabled = true;
                }
                else
                {
                    ErrorLabel.IsVisible = false;
                    StartButton.IsEnabled = false;
                }
            }
            else
            {
                ErrorLabel.IsVisible = false;
                StartButton.IsEnabled = false;
            }
        }

        private void OnCloseRequested(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnStartRequested(object sender, EventArgs e)
        {
            if (int.TryParse(QuantityEntry.Text, out int qty) && qty > 0 && qty <= _maxPossible)
            {
                StartProductionRequested?.Invoke(this, qty);
            }
        }
    }
}
