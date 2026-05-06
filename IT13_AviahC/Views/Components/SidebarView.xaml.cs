using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace IT13_AviahC.Views.Components
{
    public partial class SidebarView : ContentView
    {
        public static readonly BindableProperty ModuleTitleProperty = 
            BindableProperty.Create(nameof(ModuleTitle), typeof(string), typeof(SidebarView), "MODULES");

        public string ModuleTitle
        {
            get => (string)GetValue(ModuleTitleProperty);
            set => SetValue(ModuleTitleProperty, value);
        }

        public static readonly BindableProperty CurrentPageProperty =
            BindableProperty.Create(nameof(CurrentPage), typeof(string), typeof(SidebarView), string.Empty);

        public string CurrentPage
        {
            get => (string)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        public ICommand NavigateCommand { get; }

        public SidebarView()
        {
            NavigateCommand = new Command<object>(async (param) => 
            {
                try
                {
                    string route = param as string ?? string.Empty;
                    System.Diagnostics.Debug.WriteLine($"[Sidebar] Attempting navigation to: {route}");
 
                    if (string.IsNullOrEmpty(route)) return;
                    
                    // Use // for reliable sibling navigation within the Shell tree
                    await Shell.Current.GoToAsync($"//{route}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Sidebar] Navigation error for {param}: {ex.Message}");
                }
            });
            InitializeComponent();
            // BindingContext should not be set to 'this' in a ContentView to avoid memory leaks.
            // Instead, use x:Reference in XAML or set the content's BindingContext.
        }
    }
}
