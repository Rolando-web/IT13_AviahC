using Microsoft.Maui.Controls;

namespace IT13_AviahC.Views.Admin.Components;

public partial class AdminHeaderView : ContentView
{
    public AdminHeaderView()
    {
        InitializeComponent();
        var role = Services.UserSession.Role ?? "Admin";
        UserRoleLabel.Text = role;
        ProfileImage.Source = $"https://ui-avatars.com/api/?name={role}&background=624890&color=fff";
    }
}
