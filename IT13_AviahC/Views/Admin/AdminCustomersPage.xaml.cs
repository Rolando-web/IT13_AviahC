using System.Data;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin
{
    public partial class AdminCustomersPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public AdminCustomersPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadCustomers();
        }

        private async void LoadCustomers()
        {
            try
            {
                DataTable dt = _dbService.GetAllCustomers();
                var customers = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    customers.Add(new
                    {
                        FullName = row["FullName"].ToString(),
                        Email = row["Email"].ToString(),
                        Phone = row["Phone"].ToString(),
                        Address = row["Address"].ToString(),
                        Status = row["Status"].ToString()
                    });
                }

                CustomersCollection.ItemsSource = customers;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", "Failed to load customers: " + ex.Message, "OK");
            }
        }
    }
}
