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

            try 
            {
                // SECURE: This uses parameterized queries internally
                DataRow? user = _databaseService.GetUserByEmailAndPassword(email, password);

                // Development/Test login for Customer@aviah.com
                if (!string.IsNullOrEmpty(email) && email.Equals("Customer@aviah.com", StringComparison.OrdinalIgnoreCase))
                {
                    UserSession.UserName = "Aviah Customer";
                    UserSession.UserEmail = email;
                    UserSession.Role = "Customer";
                    await DisplayAlertAsync("Success", "Welcome back, Customer!", "OK");
                    await Shell.Current.GoToAsync("///CustomerBoutique");
                    return;
                }

                if (user != null)
                {
                    string role = user["Role"].ToString() ?? "";
                    string firstName = user["FirstName"].ToString() ?? "";
                    string lastName = user["LastName"]?.ToString() ?? "";
                    string tier = "Basic";
                    
                    // Safety check for new columns
                    if (user.Table.Columns.Contains("SubscriptionTier"))
                    {
                        tier = user["SubscriptionTier"]?.ToString() ?? "Basic";
                    }
                    
                    UserSession.UserId = Convert.ToInt32(user["Id"]);
                    UserSession.CompanyId = user.Table.Columns.Contains("CompanyId") && user["CompanyId"] != DBNull.Value 
                        ? Convert.ToInt32(user["CompanyId"]) : null;
                    
                    UserSession.UserName = $"{firstName} {lastName}".Trim();
                    UserSession.UserEmail = email;
                    UserSession.Role = role;
                    UserSession.CurrentTier = tier;

                    if (tier == "Premium")
                    {
                        try { _databaseService.RecordLoginSession(UserSession.UserId); } catch { }
                    }

                    await DisplayAlertAsync("Success", $"Welcome back, {firstName}! (Tier: {tier})", "OK");

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
                        if (UserSession.CurrentTier != "Premium")
                        {
                            await DisplayAlertAsync("Subscription Required", "The Customer Portal is only available in the Premium Tier.", "OK");
                            return;
                        }
                        await Shell.Current.GoToAsync("///CustomerBoutique");
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
            catch (Exception ex)
            {
                await DisplayAlertAsync("Critical Error", $"Login process failed: {ex.Message}\n\nStack: {ex.StackTrace}", "OK");
            }
        }
        private async void OnSignupLabelTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Signup");
        }
    }
}
