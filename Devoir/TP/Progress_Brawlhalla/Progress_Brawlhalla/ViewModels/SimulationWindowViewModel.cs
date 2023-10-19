using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Progress_Brawlhalla.Models;
using Progress_Brawlhalla.Services;
using Progress_Brawlhalla.ViewModels;

public class SimulationWindowViewModel : INotifyPropertyChanged
{
    private readonly IGameService _gameService;
    private readonly CharacterService _characterService;
    private int _characterId;
    private ObservableCollection<string> _feedbackMessages = new ObservableCollection<string>();
    private int _progressValue;
    private int _overallProgressValue;
    private int _questCount = 20;  // Default value of 1, can be adjusted based on requirements
    private Character _selectedCharacter;
    
    private ObservableCollection<CharacterQuest> _characterQuests = new ObservableCollection<CharacterQuest>();
    private ObservableCollection<Monster> _questMonsters = new ObservableCollection<Monster>();
    private ObservableCollection<Spell> _characterSpell = new ObservableCollection<Spell>();
    private string _questDescription;
    private int _rewardMoney;
    private int _rewardExperience;
    private int _equipmentRewardId;
    private ObservableCollection<Quest> _allQuests;
    private ObservableCollection<CharacterEquipment> _characterEquipment;
    public string WeaponSlot { get; set; }
    public string ArmourSlot { get; set; }
    public string HelmetSlot { get; set; }
    public string BootsSlot { get; set; }

    public ObservableCollection<CharacterEquipment> CharacterEquipments
    {
        get => _characterEquipment;
        set
        {
            _characterEquipment = value;
            OnPropertyChanged(nameof(CharacterEquipments));

            foreach (var equipment in _characterEquipment )
            {
                switch (equipment.Category)
                {
                    case "weapon":
                        WeaponSlot = equipment.Slot;
                        break;
                    case "armour":
                        ArmourSlot = equipment.Slot;
                        break;
                    case "helmet":
                        HelmetSlot = equipment.Slot;
                        break;
                    case "boots":
                        BootsSlot = equipment.Slot;
                        break;
                }
            }

            // Notify changes for each slot.
            OnPropertyChanged(nameof(WeaponSlot));
            OnPropertyChanged(nameof(ArmourSlot));
            OnPropertyChanged(nameof(HelmetSlot));
            OnPropertyChanged(nameof(BootsSlot));
        }
    }

    public ObservableCollection<Quest> AllQuests
    {
        get => _allQuests;
        set
        {
            if (_allQuests != value)
            {
                _allQuests = value;
                OnPropertyChanged(nameof(AllQuests));
            }
        }
    }
    
    
    public string QuestDescription
    {
        get => _questDescription;
        set
        {
            _questDescription = value;
            OnPropertyChanged(nameof(QuestDescription));
        }
    }

    public int EquipmentRewardId
    {
        get => _equipmentRewardId;
        set
        {
            _equipmentRewardId = value;
            OnPropertyChanged(nameof(EquipmentRewardId));
        }
    }

    public int RewardMoney
    {
        get => _rewardMoney;
        set
        {
            _rewardMoney = value;
            OnPropertyChanged(nameof(RewardMoney));
        }
    }

    public int RewardExperience
    {
        get => _rewardExperience;
        set
        {
            _rewardExperience = value;
            OnPropertyChanged(nameof(RewardExperience));
        }
    }
    
    
    


    public SimulationWindowViewModel(IGameService gameService, CharacterService characterService)
    {
        _gameService = gameService;
        _characterService = characterService;
        InitializeAllQuests();
    }


    
    private async void InitializeAllQuests()
    {
        AllQuests = new ObservableCollection<Quest>(await _gameService.GetAllQuestsAsync());
    }
    public Character SelectedCharacter
    {
        get => _selectedCharacter;
        set
        {
            _selectedCharacter = value;

            // Notify the UI that the SelectedCharacter has changed.
            OnPropertyChanged(nameof(SelectedCharacter));
        }
    }


    
    public ObservableCollection<CharacterQuest> CharacterQuests
    {
        get => _characterQuests;
        set
        {
            _characterQuests = value;
            OnPropertyChanged(nameof(CharacterQuests));
        }
    }


    public ObservableCollection<Monster> QuestMonstersList
    {
        get => _questMonsters;
        set
        {
            _questMonsters = value;
            OnPropertyChanged(nameof(QuestMonstersList));
        }
    }

    public ObservableCollection<Spell> CharacterSpells
    {
        get => _characterSpell;
        set
        {
            _characterSpell = value;
            OnPropertyChanged(nameof(CharacterSpells));
        }
    }


    public int CharacterId
    {
        get => _characterId;
        set
        {
            _characterId = value;

            // Fetch the character details when the character ID changes.
            SelectedCharacter = _characterService.GetCharacterById(_characterId);

            // Fetch quests for the character and update ObservableCollection
            var quests = _characterService.GetQuestsByCharacterId(_characterId);
            foreach (var quest in quests)
            {
                CharacterQuests.Add(quest);
            }
            OnPropertyChanged(nameof(CharacterId));
            
        }
    }
    


    public ObservableCollection<string> FeedbackMessages
    {
        get => _feedbackMessages;
        set
        {
            _feedbackMessages = value;
            OnPropertyChanged(nameof(FeedbackMessages));
        }
    }

    public int ProgressValue
    {
        get => _progressValue;
        set
        {
            _progressValue = value;
            OnPropertyChanged(nameof(ProgressValue));
        }
    }

    public int OverallProgressValue
    {
        get => _overallProgressValue;
        set
        {
            _overallProgressValue = value;
            OnPropertyChanged(nameof(OverallProgressValue));
        }
    }  

    public ICommand StartSimulationCommand => new RelayCommand(StartSimulation);

    public async void StartSimulation(object obj)
    {
        if (_characterId <= 0)
        {
            FeedbackMessages.Clear();
            FeedbackMessages.Add("Please select a valid character ID.");
            return;
        }

        var selectedCharacter = _characterService.GetCharacterById(_characterId);
        
        if (selectedCharacter == null)
        {
            FeedbackMessages.Clear();
            FeedbackMessages.Add("Character not found!");
            return;
        }

        HashSet<int> addedQuestIds = new HashSet<int>();
        
        for (int i = 0; i < _questCount; i++)
        {
            FeedbackMessages.Add($"--- Starting Quest Iteration {i + 1} ---");

            var feedbacksForCurrentQuest = await _gameService.RunSingleQuestSimulationAsync(selectedCharacter);
            double progressIncrementForCurrentQuest = 100.0 / feedbacksForCurrentQuest.Count;
            OverallProgressValue = 0;
            var quests = await _gameService.GetQuestsForCharacterAsync(_characterId);
            var currentQuest = quests.FirstOrDefault(q => q.Status == "assigned" || q.Status == "active"); 

            foreach (var quest in quests)
            {
                if (!addedQuestIds.Contains(quest.QuestId))
                {
                    CharacterQuests.Add(quest);
                    addedQuestIds.Add(quest.QuestId);
                }
            }
            // Clear existing quests from the ObservableCollection
            CharacterQuests.Clear();
            foreach (var quest in quests)
            {
                CharacterQuests.Add(quest);
            }
            if (currentQuest != null)
            {
                var questDetails = await _gameService.GetQuestDetailsByQuestId(currentQuest.QuestId);
                QuestDescription = questDetails.Description;
                RewardMoney = questDetails.RewardMoney;
                EquipmentRewardId = questDetails.Id;
                RewardExperience = questDetails.RewardExperience;
                
                var monstersForQuest = await _gameService.GetListMonstersForTempQuestAsync(currentQuest.QuestId);
                QuestMonstersList.Clear();
                foreach (var monster in monstersForQuest)
                {
                    QuestMonstersList.Add(monster);
                }

                var characterSpells = await _gameService.GetSpellsForCharacterAsync(_characterId);
                foreach (var spell in CharacterSpells)
                {
                    CharacterSpells.Add(spell);
                }
            }
            else
            {
                var questDetails = "none";
                QuestDescription = "none";
                RewardMoney = 0;
                RewardExperience = 0;
                EquipmentRewardId = 0;
            }
            // Only populate CharacterQuests if it's empty
            if (!CharacterQuests.Any())
            {
                foreach (var quest in quests)
                {
                    CharacterQuests.Add(quest);
                }
            }
            foreach (var feedback in feedbacksForCurrentQuest)
            {
                FeedbackMessages.Clear();
                FeedbackMessages.Add(feedback); 

                int targetProgressValue = GetTargetProgressValue(feedback);
                await SmoothlyUpdateProgressBar(targetProgressValue); 

                if (targetProgressValue > 0) 
                {
                    OverallProgressValue += (int)Math.Round(progressIncrementForCurrentQuest);
                }

                if (feedback.Contains("All monsters defeated") || feedback.Contains("Failed to complete quest. Defeated"))
                {
                    if (feedback.Contains("Defeated monster:"))
                    {
                        // Remove a monster from the list since it was defeated
                        if (QuestMonstersList.Any())
                        {
                            QuestMonstersList.RemoveAt(0);
                        }
                    }
                    OverallProgressValue = 0; 

                    // Fetch updated character details after this quest
                    SelectedCharacter = _characterService.GetCharacterById(_characterId);
                    // Assuming you have a method in your service to get equipments by character ID.
                    var equipments = _characterService.GetCharacterEquipmentsById(_characterId);
    
                    // Update the ObservableCollection property.
                    CharacterEquipments = new ObservableCollection<CharacterEquipment>(equipments);
                    progressIncrementForCurrentQuest = 100.0 / (feedbacksForCurrentQuest.Count - feedbacksForCurrentQuest.IndexOf(feedback) - 1);
                }

                await Task.Delay(1000); // Waits for 1 second before adding the next feedback
            }
        }
    }
    
    



    private int GetTargetProgressValue(string feedback)
    {
        if (feedback.Contains("Fighting monster"))
            return 1;
        else if (feedback.Contains("Character attacks for"))
            return 1; // Character's attack could also be an incremental progress
        else if (feedback.Contains("Monster attacks for"))
            return 2; // Monster's attack could also be an incremental progress
        else if (feedback.Contains("You emerged victorious in the battle!") || feedback.Contains(" Defeat by"))
            return 3;
        else if (feedback.Contains("Defeated monster:") || feedback.Contains("You were defeated in the battle. Prepare and try again!"))
            return 4; // Increment the progress bar to its maximum after the fight ends
        return 0;
    }


    private async Task SmoothlyUpdateProgressBar(int targetValue)
    {
        double currentProgress = ProgressValue;
        while (currentProgress < targetValue)
        {
            currentProgress += 0.01; // Smaller increment
            ProgressValue = (int)Math.Round(currentProgress);
            await Task.Delay(10); // Shorter delay
        }
        ProgressValue = targetValue;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
