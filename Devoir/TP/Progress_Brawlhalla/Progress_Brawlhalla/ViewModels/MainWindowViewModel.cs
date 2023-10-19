using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using Progress_Brawlhalla.Models;
using Progress_Brawlhalla.Services;
using ReactiveUI;
using Npgsql;

namespace Progress_Brawlhalla.ViewModels
{
    
    public class MainWindowViewModel : ViewModelBase
    {
        public ICommand AddCharacterCommand { get; }


        private Character _selectedCharacter;
        private string _characterName;
        private string _selectedCharacterClass;

        public bool IsGoldKnightSelected { get; set; }
        public bool IsSpeedsterSelected { get; set; }
        public bool IsMageSelected { get; set; }
        public bool IsOrgSelected { get; set; }

        public string SelectedCharacterClass
        {
            get
            {
                if (IsGoldKnightSelected) return "Gold Knight";
                if (IsSpeedsterSelected) return "Speedster";
                if (IsMageSelected) return "Mage";
                if (IsOrgSelected) return "Org";
                return null;
            }
            set
            {
                IsGoldKnightSelected = value == "Gold Knight";
                IsSpeedsterSelected = value == "Speedster";
                IsMageSelected = value == "Mage";
                IsOrgSelected = value == "Org";
            }
        }

        public ObservableCollection<Character> Characters { get; set; }

        public SimulationWindowViewModel SimulationVM { get; set; } // Add this line
        public string CharacterName
        {
            get => _characterName;
            set => this.RaiseAndSetIfChanged(ref _characterName, value);
        }

        


        public MainWindowViewModel()
        {
        
            var characterService = new CharacterService(); 
            Characters = new ObservableCollection<Character>(characterService.GetAllCharacters());
            IGameService gameService = new GameService();
            LoadCharacters();

            SimulationVM = new SimulationWindowViewModel(gameService, characterService);         
            AddCharacterCommand = ReactiveCommand.Create(AddCharacter);
        }
        
        private void LoadCharacters()
        {
            var characterService = new CharacterService();
            Characters.Clear();
            foreach(var character in characterService.GetAllCharacters())
            {
                Characters.Add(character);
            }
        }

        private void AddCharacter()
        {
            if (!string.IsNullOrWhiteSpace(CharacterName) && !string.IsNullOrWhiteSpace(SelectedCharacterClass))
            {
                var newCharacter = new Character
                {
                    Name = CharacterName,
                    Class = SelectedCharacterClass,
                };

                var characterService = new CharacterService();
                characterService.AddCharacter(newCharacter);

                LoadCharacters();  // Refresh the characters list after adding a new one
            }
        }

        public Character SelectedCharacter
        {
            get => _selectedCharacter;
            set => this.RaiseAndSetIfChanged(ref _selectedCharacter, value);
        }

        
    }
    
    
}