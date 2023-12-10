using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using TP2_Interface.Models;
using TP2_Interface.ViewModels;

namespace TP2_Interface.Views;

public partial class MainWindow : Window
{
    public MainWindow(string postgresUsername, string postgresPassword, bool isAdmin)
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        DataContext = new MainWindowViewModel(postgresUsername, postgresPassword, isAdmin);
        // Wire up the DoubleTapped event after InitializeComponent
        var listBox = this.FindControl<ListBox>("KnowledgeEntriesListBox");
        listBox.DoubleTapped += ListBox_DoubleTapped;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ListBox_DoubleTapped(object sender, RoutedEventArgs e)
    {
        // Use sender to get the ListBox
        var listBox = sender as ListBox;
        // Get the selected item directly from the ListBox
        if (listBox.SelectedItem is KnowledgeEntry entry)
        {
            var detailWindow = new DetailWindow(entry);
            detailWindow.Show();
        }
    }
}