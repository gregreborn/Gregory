using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TP2_Interface.ViewModels; // Assuming this is the namespace for your view models

namespace TP2_Interface.Views;

public partial class AdminWindow : Window
{
    public AdminWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        DataContext = new AdminViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}