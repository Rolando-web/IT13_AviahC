using System.Data;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin;

public partial class AdminSuppliersPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public AdminSuppliersPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadSuppliers();
    }

    private async void LoadSuppliers()
    {
        try
        {
            DataTable dt = _dbService.GetAllSuppliers();
            var suppliers = new List<object>();

            foreach (DataRow row in dt.Rows)
            {
                suppliers.Add(new
                {
                    CompanyName = row["CompanyName"].ToString(),
                    ContactPerson = row["ContactPerson"].ToString(),
                    Email = row["Email"].ToString(),
                    Category = row["Category"].ToString(),
                    Status = row["Status"].ToString()
                });
            }

            SuppliersCollection.ItemsSource = suppliers;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "Failed to load suppliers: " + ex.Message, "OK");
        }
    }
}
