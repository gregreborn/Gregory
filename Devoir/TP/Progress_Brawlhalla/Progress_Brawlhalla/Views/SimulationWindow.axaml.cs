using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Progress_Brawlhalla.Models;
using Progress_Brawlhalla.Services;


namespace Progress_Brawlhalla.Views
{
    public partial class SimulationWindow : Window
    {
        public SimulationWindow()
        {
            InitializeComponent();

            // Provide both services to the ViewModel
            var gameService = new GameService();
            var characterService = new CharacterService();
            DataContext = new SimulationWindowViewModel(gameService, characterService);

            #if DEBUG
            this.AttachDevTools();
            #endif
        }
        public SimulationWindow(string initialMessage)
        {
            DataContext = new SimulationWindowViewModel(new GameService(), new CharacterService());
            InitializeComponent();
            
    
            // Now, you can set this message somewhere in your ViewModel or display it in some TextBlock in your SimulationWindow. 
            // This depends on how you want to handle this initialMessage in your simulation.
        }



        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}