using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Progress_Brawlhalla.Services;
using Progress_Brawlhalla.ViewModels;
using Progress_Brawlhalla.Views;

namespace Progress_Brawlhalla;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new SimulationWindow()
            {
                DataContext = new SimulationWindowViewModel(new GameService(), new CharacterService()),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}