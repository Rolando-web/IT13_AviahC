using IT13_AviahC.Services;
using System.Data;

namespace IT13_AviahC.Views.Modals
{
    public partial class AdminUserModal : ContentPage
    {
        private readonly DatabaseService _dbService;
        public event EventHandler? UserAdded;

        public AdminUserModal()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            SetupRolePicker();
        }

        private void SetupRolePicker()
        {
            string tier = (UserSession.CurrentTier ?? "Basic").Trim();
            RolePicker.Items.Clear();

            // All admins can add Staff and Suppliers
            RolePicker.Items.Add("Staff");
            RolePicker.Items.Add("Supplier");

            // Premium admins unlock DeliveryStaff and Customer
            if (tier.Equals("Premium", StringComparison.OrdinalIgnoreCase))
            {
                RolePicker.Items.Add("DeliverStaff");
                RolePicker.Items.Add("Customer");
                TierContextLabel.Text = "Premium Tier (All Roles Unlocked)";
            }
            else
            {
                TierContextLabel.Text = "Standard Tier (Limited Roles)";
            }
            
            RolePicker.SelectedIndex = 0;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) || 
                string.IsNullOrWhiteSpace(LastNameEntry.Text) || 
                string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlertAsync("Required Fields", "Please fill in all details, including a password.", "OK");
                return;
            }

            string role = RolePicker.SelectedItem?.ToString() ?? "Staff";
            
            try 
            {
                // Check if email exists
                DataTable dt = await _dbService.ExecuteQueryAsync("SELECT 1 FROM Users WHERE Email = @Email", 
                    new Dictionary<string, object> { { "@Email", EmailEntry.Text } });

                if (dt != null && dt.Rows.Count > 0)
                {
                    await DisplayAlertAsync("Duplicate User", "A user with this email already exists.", "OK");
                    return;
                }

                // Insert user
                string query = @"
                    INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role, SubscriptionTier, CompanyId)
                    VALUES (@FN, @LN, @Email, @Password, @Role, @Tier, @CompanyId)";

                var parameters = new Dictionary<string, object>
                {
                    { "@FN", FirstNameEntry.Text },
                    { "@LN", LastNameEntry.Text },
                    { "@Email", EmailEntry.Text },
                    { "@Password", PasswordEntry.Text },
                    { "@Role", role },
                    { "@Tier", UserSession.CurrentTier ?? "Basic" },
                    { "@CompanyId", UserSession.CompanyId ?? (object)DBNull.Value }
                };

                int result = await _dbService.ExecuteNonQueryAsync(query, parameters);

                if (result > 0)
                {
                    UserAdded?.Invoke(this, EventArgs.Empty);
                    await Navigation.PopModalAsync();
                }
                else
                {
                    await DisplayAlertAsync("Error", "Failed to create user.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("System Error", ex.Message, "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
