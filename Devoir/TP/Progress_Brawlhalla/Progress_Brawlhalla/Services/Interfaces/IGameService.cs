using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Progress_Brawlhalla.Models;

public interface IGameService
{
    Task GenerateAndAssignQuestAsync(int characterId);
    Task<IEnumerable<string>> GetCombatFeedbackMessagesAsync(int characterId, int questId);
    Task<List<CharacterEquipment>> GetEquipmentForCharacterAsync(int characterId);
    Task<List<CharacterQuest>> GetQuestsForCharacterAsync(int characterId);
    Task<Quest> GetQuestDetailsByQuestId(int characterId);
    Task<List<Spell>> GetSpellsForCharacterAsync(int characterId);
    Task<List<QuestMonsters>> GetMonstersForQuestAsync(int questId);
    Task<List<Monster>> GetListMonstersForTempQuestAsync(int questId);
    Task<Tuple<string, int?>> StartQuestAsync(int characterId);
    string ProvideQuestFeedback(string combatResult);
    Task<ObservableCollection<string>> RunQuestSimulationAsync(Character character, int iterations = 5);
    Task<ObservableCollection<string>> RunSingleQuestSimulationAsync(Character character);
    Task<string> AwardAndCompleteQuest(int characterId);
    string FetchLatestCharacterDetails(int characterId);
    Task<List<Quest>> GetAllQuestsAsync();
}