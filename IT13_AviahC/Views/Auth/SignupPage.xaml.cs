using IT13_AviahC.Services;
using System.Data;

namespace IT13_AviahC.Views.Auth
{
    public partial class SignupPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public SignupPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        private async void OnSignupClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) || 
                string.IsNullOrWhiteSpace(LastNameEntry.Text) || 
                string.IsNullOrWhiteSpace(EmailEntry.Text) || 
                string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlertAsync("Required Fields", "Please fill in all details.", "OK");
                return;
            }

            try 
            {
                // Check if email exists
                DataTable dt = await _dbService.ExecuteQueryAsync("SELECT 1 FROM Users WHERE Email = @Email", 
                    new Dictionary<string, object> { { "@Email", EmailEntry.Text } });

                if (dt != null && dt.Rows.Count > 0)
                {
                    await DisplayAlertAsync("Duplicate Account", "An account with this email already exists.", "OK");
                    return;
                }

                // Insert customer user
                string query = @"
                    INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role, SubscriptionTier, CompanyId)
                    VALUES (@FN, @LN, @Email, @Password, 'Customer', 'Basic', NULL)";

                var parameters = new Dictionary<string, object>
                {
                    { "@FN", FirstNameEntry.Text },
                    { "@LN", LastNameEntry.Text },
                    { "@Email", EmailEntry.Text },
                    { "@Password", PasswordEntry.Text }
                };

                int result = await _dbService.ExecuteNonQueryAsync(query, parameters);

                if (result > 0)
                {
                    await DisplayAlertAsync("Success", "Account created successfully! You can now log in.", "OK");
                    await Shell.Current.GoToAsync("//Login");
                }
                else
                {
                    await DisplayAlertAsync("Error", "Failed to create account. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("System Error", ex.Message, "OK");
            }
        }

        private async void OnLoginLabelTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Login");
        }
    }
}
