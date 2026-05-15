using System.Data;
using System.Collections.ObjectModel;
using IT13_AviahC.Services;

namespace IT13_AviahC.Views.Admin;

public class UserDisplayItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string SubscriptionTier { get; set; } = string.Empty;
    public string Initials => string.IsNullOrEmpty(FullName) ? "?" : FullName[0].ToString().ToUpper();
}

public class SessionDisplayItem
{
    public string UserName { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public string LoginTimeFormatted => LoginTime.ToString("MMM dd, hh:mm tt");
}

public partial class AdminManageUsersPage : ContentPage
{
    private readonly DatabaseService _dbService;
    public ObservableCollection<UserDisplayItem> Users { get; } = new();
    public ObservableCollection<SessionDisplayItem> ActiveSessions { get; } = new();

    public AdminManageUsersPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        UsersCollection.ItemsSource = Users;
        SessionsCollection.ItemsSource = ActiveSessions;

        ConfigureTierAccess();
    }

    private void ConfigureTierAccess()
    {
        string tier = UserSession.CurrentTier ?? "Basic";
        
        if (tier == "Standard")
        {
            TierScopeLabel.Text = "Managing company-wide staff and accounts";
            SessionsSection.IsVisible = false;
        }
        else if (tier == "Premium")
        {
            TierScopeLabel.Text = "Enterprise control: Staff, Logistics, Suppliers, and Customers";
            SessionsSection.IsVisible = true;
        }
        else
        {
            // Basic shouldn't even get here due to sidebar logic, but safety first
            TierScopeLabel.Text = "Read-only access (Upgrade to manage)";
            SessionsSection.IsVisible = false;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            Users.Clear();
            DataTable dt;

            if (UserSession.CurrentTier == "Premium")
            {
                // Tier 3: All related users
                dt = await _dbService.ExecuteQueryAsync("SELECT Id, FirstName, LastName, Email, Role, SubscriptionTier FROM Users WHERE Role IN ('Staff', 'DeliverStaff', 'DeliveryStaff', 'Supplier', 'Customer', 'Admin')");
                
                // Load sessions too
                LoadSessions();
            }
            else
            {
                // Tier 2: Company only
                dt = await _dbService.ExecuteQueryAsync("SELECT Id, FirstName, LastName, Email, Role, SubscriptionTier FROM Users WHERE CompanyId = @CompanyId", 
                    new Dictionary<string, object> { { "@CompanyId", UserSession.CompanyId ?? 0 } });
            }

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Users.Add(new UserDisplayItem
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        FullName = $"{row["FirstName"]} {row["LastName"]}",
                        Email = row["Email"]?.ToString() ?? "",
                        Role = row["Role"]?.ToString() ?? "",
                        SubscriptionTier = row["SubscriptionTier"]?.ToString() ?? "Basic"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "Failed to load users: " + ex.Message, "OK");
        }
    }

    private async void LoadSessions()
    {
        try
        {
            ActiveSessions.Clear();
            DataTable dt = await _dbService.ExecuteQueryAsync(@"
                SELECT u.FirstName + ' ' + u.LastName AS UserName, s.LoginTime, s.DeviceType 
                FROM UserSessions s 
                INNER JOIN Users u ON s.UserId = u.Id 
                ORDER BY s.LoginTime DESC");

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    ActiveSessions.Add(new SessionDisplayItem
                    {
                        UserName = row["UserName"]?.ToString() ?? "Unknown",
                        LoginTime = row["LoginTime"] != DBNull.Value ? (DateTime)row["LoginTime"] : DateTime.Now,
                        DeviceType = row["DeviceType"]?.ToString() ?? "Desktop"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Session load error: " + ex.Message);
        }
    }

    private async void OnAddUserClicked(object sender, EventArgs e)
    {
        var modal = new Modals.AdminUserModal();
        modal.UserAdded += (s, e) => { LoadData(); };
        await Navigation.PushModalAsync(modal);
    }
}
