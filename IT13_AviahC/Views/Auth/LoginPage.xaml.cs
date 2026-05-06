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

            if (user != null)
            {
                string role = user["Role"].ToString() ?? "";
                string firstName = user["FirstName"].ToString() ?? "";

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
                        await Shell.Current.GoToAsync("//CustomerPortal/CustomerBoutique");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlertAsync("Navigation Error", $"Could not go to Boutique: {ex.Message}", "OK");
                    }
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
