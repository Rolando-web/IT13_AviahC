using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace IT13_AviahC.Views.Admin.Components
{
    public partial class AdminSidebarView : ContentView
    {
        public static readonly BindableProperty CurrentPageProperty =
            BindableProperty.Create(nameof(CurrentPage), typeof(string), typeof(AdminSidebarView), string.Empty);

        public string CurrentPage
        {
            get => (string)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        private bool _isNavigating;
        public ICommand NavigateCommand { get; }

        public AdminSidebarView()
        {
            NavigateCommand = new Command<object>(async (param) => 
            {
                if (_isNavigating) return;
                
                try
                {
                    _isNavigating = true;
                    string route = param as string ?? string.Empty;
                    System.Diagnostics.Debug.WriteLine($"[AdminSidebar] Attempting navigation to: {route}");
                    
                    if (string.IsNullOrEmpty(route)) return;
                    
                    // Use // for reliable sibling navigation within the Shell tree
                    await Shell.Current.GoToAsync($"//{route}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminSidebar] Navigation error for {param}: {ex.Message}");
                }
                finally
                {
                    _isNavigating = false;
                }
            });
            InitializeComponent();
            // BindingContext should not be set to 'this' in a ContentView to avoid memory leaks.
            // Instead, use x:Reference in XAML or set the content's BindingContext.
        }
    }
}
