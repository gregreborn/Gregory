using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TP2_Interface.ViewModels;

namespace TP2_Interface.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        DataContext = new LoginViewModel(this);
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}