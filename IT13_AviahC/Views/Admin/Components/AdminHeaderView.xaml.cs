using Microsoft.Maui.Controls;

namespace IT13_AviahC.Views.Admin.Components;

public partial class AdminHeaderView : ContentView
{
    public AdminHeaderView()
    {
        InitializeComponent();
        this.Loaded += (s, e) => RefreshHeader();
        RefreshHeader();
    }

    private void RefreshHeader()
    {
        var role = Services.UserSession.Role ?? "Admin";
        var tier = Services.UserSession.CurrentTier ?? "Basic";
        UserRoleLabel.Text = role;
        UserTierLabel.Text = $"{tier} Tier";
        ProfileImage.Source = $"https://ui-avatars.com/api/?name={role}&background=624890&color=fff";
    }
}
