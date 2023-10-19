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
        var viewModel = DataContext as MainWindowViewModel;
    
        if (viewModel != null && viewModel.SelectedCharacter != null)
        {
            // Pass the selected character's ID to the SimulationWindow constructor
            var simulationWindow = new SimulationWindow(viewModel.SelectedCharacter.Id.ToString());
            
            
            simulationWindow.Show();
            
            viewModel.SimulationVM.StartSimulationCommand.Execute(null);

        }

        // Optionally close the MainWindow if you want the simulation to take over completely
        this.Close();
    }

    private void AddCharacterButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        if (viewModel != null)
        {
            viewModel.AddCharacterCommand.Execute(null);
        }
    }
}