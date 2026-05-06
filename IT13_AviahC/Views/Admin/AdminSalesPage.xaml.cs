using System.Data;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin
{
    public partial class AdminSalesPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public AdminSalesPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadSales();
        }

        private async void LoadSales()
        {
            try
            {
                DataTable dt = _dbService.GetAllSales();
                var sales = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    string status = row["Status"]?.ToString() ?? string.Empty;
                    string statusColorDark = "#624890";
                    string statusColorLight = "#F3F0FA";

                    if (status == "Completed")
                    {
                        statusColorDark = "#15803D";
                        statusColorLight = "#DCFCE7";
                    }
                    else if (status == "Processing")
                    {
                        statusColorDark = "#1D4ED8";
                        statusColorLight = "#DBEAFE";
                    }
                    else if (status == "Shipped")
                    {
                        statusColorDark = "#92400E";
                        statusColorLight = "#FEF3C7";
                    }

                    sales.Add(new
                    {
                        OrderID = row["OrderID"]?.ToString() ?? string.Empty,
                        CustomerName = row["CustomerName"]?.ToString() ?? string.Empty,
                        ItemsSummary = row["ItemsSummary"]?.ToString() ?? string.Empty,
                        DateTime = row["SalesDate"]?.ToString() ?? string.Empty,
                        Amount = Convert.ToDecimal(row["TotalAmount"] ?? 0),
                        Status = status,
                        StatusColorDark = statusColorDark,
                        StatusColorLight = statusColorLight
                    });
                }

                SalesCollection.ItemsSource = sales;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", "Failed to load sales: " + ex.Message, "OK");
            }
        }
    }
}
