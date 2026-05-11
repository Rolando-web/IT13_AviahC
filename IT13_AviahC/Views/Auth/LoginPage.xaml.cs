using IT13_AviahC.Services;
using System.Data;

namespace IT13_AviahC.Views.Auth
{
    public partial class LoginPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public LoginPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        private async void OnLoginClicked(object? sender, EventArgs e)
        {
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlertAsync("Error", "Please enter both email and password.", "OK");
                return;
            }

            // SECURE: This uses parameterized queries internally
            DataRow? user = _databaseService.GetUserByEmailAndPassword(email, password);

            // Development/Test login for Customer@aviah.com
            if (!string.IsNullOrEmpty(email) && email.Equals("Customer@aviah.com", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    UserSession.UserName = "Aviah Customer";
                    UserSession.UserEmail = email;
                    UserSession.Role = "Customer";
                    await DisplayAlertAsync("Success", "Welcome back, Customer!", "OK");
                    await Shell.Current.GoToAsync("///CustomerBoutique");
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("Navigation Error", ex.Message, "OK");
                }
                return;
            }

            if (user != null)
            {
                string role = user["Role"].ToString() ?? "";
                string firstName = user["FirstName"].ToString() ?? "";
                string lastName = user["LastName"]?.ToString() ?? "";
                
                UserSession.UserName = $"{firstName} {lastName}".Trim();
                UserSession.UserEmail = email;
                UserSession.Role = role;

                await DisplayAlertAsync("Success", $"Welcome back, {firstName}!", "OK");

                // Navigate based on role
                if (role == "Superadmin")
                {
                    await Shell.Current.GoToAsync("///SuperadminDashboard");
                }
                else if (role == "Admin")
                {
                    await Shell.Current.GoToAsync("///AdminDashboard");
                }
                else if (role == "Customer")
                {
                    try
                    {
                        await Shell.Current.GoToAsync("///CustomerBoutique");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlertAsync("Navigation Error", $"Could not go to Boutique: {ex.Message}", "OK");
                    }
                }
                else if (role == "Staff")
                {
                    await Shell.Current.GoToAsync("///StaffDashboard");
                }
                else if (role == "Supplier")
                {
                    await Shell.Current.GoToAsync("///SupplierDashboard");
                }
                else if (role == "DeliverStaff" || role == "DeliveryStaff")
                {
                    await Shell.Current.GoToAsync("///DeliveryStaffDashboard");
                }
                else
                {
                    await DisplayAlertAsync("Access Denied", "Your role does not have access to this portal.", "OK");
                }
            }
            else
            {
                await DisplayAlertAsync("Login Failed", "Invalid email or password.", "OK");
            }
        }
    }
}
