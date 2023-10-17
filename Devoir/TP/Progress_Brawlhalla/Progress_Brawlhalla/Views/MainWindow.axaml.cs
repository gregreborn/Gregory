using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Progress_Brawlhalla.Models;
using Progress_Brawlhalla.Services;
using Progress_Brawlhalla.ViewModels;

namespace Progress_Brawlhalla.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

    }
    private void StartGameButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedCharacter = (Character)((MainWindowViewModel)DataContext).SelectedCharacter;
        var gameService = new GameService();

        var resultMessage = gameService.StartQuestAsync(selectedCharacter.Id);

        // Open the new SimulationWindow and pass the result message to it.
        // If SimulationWindow has a constructor that accepts a message, you can pass it there.
        var simulationWindow = new SimulationWindow(resultMessage.ToString());
        simulationWindow.Show();

        // Optionally close the MainWindow if you want the simulation to take over completely
        this.Close();
    }

}