using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TP2_Interface.Models;
using TP2_Interface.ViewModels;

namespace TP2_Interface.Views;

public partial class DetailWindow : Window
{
    public DetailWindow(KnowledgeEntry entry)
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        DataContext = new DetailViewModel(entry);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}