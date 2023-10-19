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
        
        public SimulationWindow(string characterIdStr)
        {
            InitializeComponent();

            int characterId = int.TryParse(characterIdStr, out int result) ? result : 0;

            // Provide both services to the ViewModel
            var gameService = new GameService();
            var characterService = new CharacterService();
            DataContext = new SimulationWindowViewModel(gameService, characterService);

            // If the parsing was successful, set the CharacterId
            if (characterId > 0)
            {
                (DataContext as SimulationWindowViewModel).CharacterId = characterId;
            }

            (DataContext as SimulationWindowViewModel).StartSimulation(null);

            #if DEBUG
            this.AttachDevTools();
            #endif
        }





        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}