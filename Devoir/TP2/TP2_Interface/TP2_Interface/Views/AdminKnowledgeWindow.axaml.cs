using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TP2_Interface.Views;

public partial class AdminKnowledgeWindow : Window
{
    public AdminKnowledgeWindow()
    {
        InitializeComponent();
    }
    // In your Avalonia window code-behind (not in ViewModel)
    public async Task ShowDialog(string text)
    {
        var dialog = new Window
        {
            // Dialog setup
        };

        await dialog.ShowDialog(this);
    }

}