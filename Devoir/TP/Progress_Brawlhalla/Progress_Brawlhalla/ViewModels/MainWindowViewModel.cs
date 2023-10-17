using System;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json.Linq;
using Progress_Brawlhalla.Models;
using Progress_Brawlhalla.Services;
using ReactiveUI;
using Npgsql;

namespace Progress_Brawlhalla.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        public ObservableCollection<Character> Characters { get; set; }

        private Character _selectedCharacter;
        

        public MainWindowViewModel()
        {
        
            var characterService = new CharacterService(); 
            Characters = new ObservableCollection<Character>(characterService.GetAllCharacters());
            
            
        }
        public Character SelectedCharacter
        {
            get => _selectedCharacter;
            set => this.RaiseAndSetIfChanged(ref _selectedCharacter, value);
        }
    }
}