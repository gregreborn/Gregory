using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Progress_Brawlhalla.Config;
using Progress_Brawlhalla.Models;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;


namespace Progress_Brawlhalla.Services
{
    public class GameService:IGameService
    {
        private readonly string ConnectionString;

        public GameService()
        {
            ConnectionString = DatabaseConfig.ConnectionString;
        }

        

        
        public async Task GenerateAndAssignQuestAsync(int characterId)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT * FROM generate_and_assign_quest(@CharacterId)", connection);
            cmd.Parameters.AddWithValue("CharacterId", characterId);

            await cmd.ExecuteNonQueryAsync();
        }
        
        public async Task<IEnumerable<string>> GetCombatFeedbackMessagesAsync(int characterId, int questId)
        {
            var feedbackMessages = new List<string>();

            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();
            connection.Notice += (o, e) => feedbackMessages.Add(e.Notice.MessageText);

            using var cmd = new NpgsqlCommand("SELECT * FROM complete_quest_with_combat(@CharacterId, @QuestId)", connection);
            cmd.Parameters.AddWithValue("CharacterId", characterId);
            cmd.Parameters.AddWithValue("QuestId", questId);

            var result = await cmd.ExecuteScalarAsync();

            if (result != null)
            {
                feedbackMessages.Add(result.ToString());
            }

            return feedbackMessages;
        }

        public async Task<List<CharacterEquipment>> GetEquipmentForCharacterAsync(int characterId)
        {
            List<CharacterEquipment> equipments = new List<CharacterEquipment>();

            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            using var cmd = new NpgsqlCommand($"SELECT id, character_id, equipment_id, slot, category FROM character_equipment WHERE character_id = @CharacterId", connection);
            cmd.Parameters.AddWithValue("CharacterId", characterId);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                equipments.Add(new CharacterEquipment
                {
                    Id = reader.GetInt32(0),
                    CharacterId = reader.GetInt32(1),
                    EquipmentId = reader.GetInt32(2),
                    Slot = reader.GetString(3),
                    Category = reader.GetString(4)
                });
            }

            return equipments;
        }


        public async Task<List<Monster>> GetListMonstersForTempQuestAsync(int questId)
        {
            var monsters = new List<Monster>();

            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            // We will join the TempMonsters table and the Monster table on MonsterId
            // Update the JOIN clause to fetch from the quest_temp_monsters table
            string query = @"
                            SELECT m.id, m.type, m.rank, m.base_hp, m.base_strength 
                            FROM quest_temp_monsters qm
                            JOIN monsters m ON qm.monster_id = m.id
                            WHERE qm.quest_id = @QuestId-1";

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("QuestId", questId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var monster = new Monster
                {
                    Id = reader.GetInt32(0),
                    Type = reader.GetString(1),
                    Rank = reader.GetString(2),
                    BaseHp = reader.GetInt32(3),
                    BaseStrength = reader.GetInt32(4)
                };
                monsters.Add(monster);
            }

            return monsters;
        }



        public async Task<Quest> GetQuestDetailsByQuestId(int questId)
        {
            Quest quest = null;

            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT id, description, reward_money, reward_experience, equipment_reward_id FROM quests WHERE id = @QuestId", connection);
            cmd.Parameters.AddWithValue("QuestId", questId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                quest = new Quest
                {
                    Id = reader.GetInt32(0),
                    Description = reader.GetString(1),
                    RewardMoney = reader.GetInt32(2),
                    RewardExperience = reader.GetInt32(3),
                    EquipmentRewardId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                };
            }

            return quest;
        }

        public async Task<List<CharacterQuest>> GetQuestsForCharacterAsync(int characterId)
        {
            List<CharacterQuest> quests = new List<CharacterQuest>();

            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT id, character_id, quest_id, status, level_when_assigned FROM character_quests WHERE character_id = @CharacterId", connection);
            cmd.Parameters.AddWithValue("CharacterId", characterId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                quests.Add(new CharacterQuest
                {
                    Id = reader.GetInt32(0),
                    CharacterId = reader.GetInt32(1),
                    QuestId = reader.GetInt32(2),
                    Status = reader.GetString(3),
                    LevelWhenAssigned = reader.GetInt32(4)
                });
            }

            return quests;
        }


        public async Task<List<Spell>> GetSpellsForCharacterAsync(int characterId)
        {
            List<Spell> spells = new List<Spell>();

            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            string query = @"
                SELECT s.name, s.effect_type 
                FROM character_spells cs 
                JOIN spells s ON cs.spell_id = s.id 
                WHERE cs.character_id = @CharacterId";

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("CharacterId", characterId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                spells.Add(new Spell
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    EffectType = reader.GetString(2),
                });
            }

            return spells;
        }


        public async Task<List<QuestMonsters>> GetMonstersForQuestAsync(int questId)
        {
            List<QuestMonsters> questMonsters = new List<QuestMonsters>();

            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT id, quest_id, monster_id, monster_count FROM quest_monsters WHERE quest_id = @QuestId", connection);
            cmd.Parameters.AddWithValue("QuestId", questId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                questMonsters.Add(new QuestMonsters
                {
                    Id = reader.GetInt32(0),
                    QuestId = reader.GetInt32(1),
                    MonsterId = reader.GetInt32(2),
                    MonsterCount = reader.GetInt32(3)
                });
            }

            return questMonsters;
        }

        
        public async Task<Tuple<string, int?>> StartQuestAsync(int characterId)
        {
            string feedback = "Starting quest for character with ID: " + characterId;
            await GenerateAndAssignQuestAsync(characterId);

            var quests = await GetQuestsForCharacterAsync(characterId);
            var assignedQuest = quests.FirstOrDefault(q => q.Status == "assigned");
    
            int? questId = null;
            if(assignedQuest != null)
            {
                feedback += "\nAssigned Quest ID: " + assignedQuest.QuestId.ToString();
                questId = assignedQuest.QuestId;
            }
            else
            {
                feedback += "\nNo assigned quest found.";
            }

            return new Tuple<string, int?>(feedback, questId);
        }



        
        

        public string ProvideQuestFeedback(string combatResult)
        {
            if (combatResult.Contains("Victory"))
            {
                return "You emerged victorious in the battle!";
            }
            else if (combatResult.Contains("Failed to defeat"))
            {
                return "You were defeated in the battle. Prepare and try again!";
            }
            else
            {
                return combatResult; // Default feedback
            }
        }

        public async Task<List<Quest>> GetAllQuestsAsync()
        {
            List<Quest> quests = new List<Quest>();

            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT id, description, reward_money, reward_experience, equipment_reward_id FROM quests", connection);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                quests.Add(new Quest
                {
                    Id = reader.GetInt32(0),
                    Description = reader.GetString(1),
                    RewardMoney = reader.GetInt32(2),
                    RewardExperience = reader.GetInt32(3),
                    EquipmentRewardId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                });
            }

            return quests;
        }


        public async Task<ObservableCollection<string>> RunSingleQuestSimulationAsync(Character character)
        {
            var feedbacks = new ObservableCollection<string>();

            // Check if the character already has quests. If not, generate the first quest.
            var existingQuests = await GetQuestsForCharacterAsync(character.Id);
            if (!existingQuests.Any())
            {
                var startQuestResult = await StartQuestAsync(character.Id);
                feedbacks.Add(startQuestResult.Item1);
                int? assignedQuestId = startQuestResult.Item2;

                if (!assignedQuestId.HasValue)
                {
                    feedbacks.Add("No quest was assigned.");
                    return feedbacks;
                }

                feedbacks.Add("Assigned Quest ID: " + assignedQuestId.Value);
            }

            feedbacks.Add("--- Starting Quest ---");

            // Re-fetch the quests to ensure the most recent one is considered.
            existingQuests = await GetQuestsForCharacterAsync(character.Id);
            var latestQuest = existingQuests.OrderByDescending(q => q.Id).FirstOrDefault();
            if (latestQuest == null)
            {
                feedbacks.Add("No quest available. Generating a new one.");
                return feedbacks;  
            }

            // Getting feedback
            var combatResults = await GetCombatFeedbackMessagesAsync(character.Id, latestQuest.QuestId);
            foreach (var feedbackMessage in combatResults)
            {
                feedbacks.Add(ProvideQuestFeedback(feedbackMessage));
            }

            if (feedbacks.Last().Contains("All monsters defeated"))
            {
                feedbacks.Add(await AwardAndCompleteQuest(character.Id));
                feedbacks.Add(await EquipBetterItem(character.Id));
                feedbacks.Add(await CheckAndLevelUp(character.Id));
            }
            else if (feedbacks.Last().Contains("Failed to defeat"))
            {
                feedbacks.Add(PenaltyForFailure(character.Id));
                feedbacks.Add(CheckAndLevelDown(character.Id));
            }

            return feedbacks;
        }

public async Task<ObservableCollection<string>> RunQuestSimulationAsync(Character character, int iterations = 5)
{
    var feedbacks = new ObservableCollection<string>();

    // Check if the character already has quests. If not, generate the first quest.
    var existingQuests = await GetQuestsForCharacterAsync(character.Id);
    if (!existingQuests.Any())
    {
        var startQuestResult = await StartQuestAsync(character.Id);
        feedbacks.Add(startQuestResult.Item1);
        int? assignedQuestId = startQuestResult.Item2;

        if (!assignedQuestId.HasValue)
        {
            feedbacks.Add("No quest was assigned.");
            return feedbacks;
        }

        feedbacks.Add("Assigned Quest ID: " + assignedQuestId.Value);
    }

    for (int i = 0; i < iterations; i++)
    {
        feedbacks.Add("--- Quest " + (i + 1).ToString() + " ---");

        // Re-fetch the quests to ensure the most recent one is considered.
        existingQuests = await GetQuestsForCharacterAsync(character.Id);
        var latestQuest = existingQuests.OrderByDescending(q => q.Id).FirstOrDefault();
        if (latestQuest == null)
        {
            feedbacks.Add("No quest available. Generating a new one.");
            continue;  // Skip the current iteration if there's no quest.
        }

        // Getting feedback
        var combatResults = await GetCombatFeedbackMessagesAsync(character.Id, latestQuest.QuestId);
        foreach (var feedbackMessage in combatResults)
        {
            feedbacks.Add(ProvideQuestFeedback(feedbackMessage));
        }

        if (feedbacks.Last().Contains("All monsters defeated"))
        {
            feedbacks.Add(await AwardAndCompleteQuest(character.Id));
            feedbacks.Add(await EquipBetterItem(character.Id));

            // Fetch the updated character details and replace the local character object.
            character = new CharacterService().GetCharacterById(character.Id);

            feedbacks.Add(await CheckAndLevelUp(character.Id));
        }
        else if (feedbacks.Last().Contains("Failed to defeat"))
        {
            feedbacks.Add(PenaltyForFailure(character.Id));
            feedbacks.Add(CheckAndLevelDown(character.Id));
        }

        await Task.Delay(2000); // Wait for 1 second
    }

    return feedbacks;
}





        
        public async Task<string> AwardAndCompleteQuest(int characterId)
        {
            // Fetch the updated character details after the quest completion
            CharacterService characterService = new CharacterService();
            Character characterDetails = characterService.GetCharacterById(characterId);

            // Fetch the equipment details
            List<CharacterEquipment> characterEquipment = await GetEquipmentForCharacterAsync(characterId);
            string latestEquipment = characterEquipment.OrderByDescending(e => e.Id).FirstOrDefault()?.EquipmentId.ToString() ?? "No new equipment";

            return $"Congratulations! You have successfully completed the quest.\n" +
                   $"Experience: {characterDetails.Experience}\n" +
                   $"Money: {characterDetails.Money}\n" +
                   $"HP: {characterDetails.Hp}/{characterDetails.MaxHp}\n" +
                   $"MP: {characterDetails.Mp}/{characterDetails.MaxMp}\n" +
                   $"Strength: {characterDetails.Strength}\n" +
                   $"Dexterity: {characterDetails.Dexterity}\n" +
                   $"Latest Equipment ID: {latestEquipment}";
        }


        public string FetchLatestCharacterDetails(int characterId)
        {
            CharacterService characterService = new CharacterService();
            Character character = characterService.GetCharacterById(characterId);

            if (character == null)
            {
                return "Character not found.";
            }

            string feedback = $"Character Details for {character.Name}:\n";
            feedback += $"- Class: {character.Class}\n";
            feedback += $"- Level: {character.Level}\n";
            feedback += $"- Experience: {character.Experience}\n";
            feedback += $"- HP: {character.Hp}/{character.MaxHp}\n";
            feedback += $"- MP: {character.Mp}/{character.MaxMp}\n";
            feedback += $"- Strength: {character.Strength}\n";
            feedback += $"- Dexterity: {character.Dexterity}\n";
            feedback += $"- Money: {character.Money}\n";

            return feedback;
        }



        public async Task<string> EquipBetterItem(int characterId)
        {
            var latestEquipment = await LatestEquipmentFeedback(characterId);
            return latestEquipment;
        }



        public async Task<string> CheckAndLevelUp(int characterId)
        {
            // Fetch the character's current stats
            CharacterService characterService = new CharacterService();
            Character characterStats = characterService.GetCharacterById(characterId);
    
            var latestQuest = (await GetQuestsForCharacterAsync(characterId)).OrderByDescending(q => q.Id).FirstOrDefault();

            if (latestQuest != null && characterStats.Level > latestQuest.LevelWhenAssigned)
            {
                return $"Congratulations! You leveled up to {characterStats.Level} from {latestQuest.LevelWhenAssigned}.\n" +
                       $"Strength: {characterStats.Strength}, Dexterity: {characterStats.Dexterity}, Max HP: {characterStats.MaxHp}.";
            }
            else
            {
                return $"You're currently at Level {characterStats.Level}. Keep pushing!";
            }
        }




        public string PenaltyForFailure(int characterId)
        {
            // This method applies penalties for failing a quest.
            return "Applying penalties for failing the quest.\nExperience deducted: 10.\nMoney deducted: 5.\nRemoved awarded equipment due to quest failure.";
        }

        public string CheckAndLevelDown(int characterId)
        {
            // Checks if the character needs to be leveled down due to penalties.
            return "Checking if character needs to be leveled down.\nCharacter has been leveled down to level: 1.\nNew Strength: 10. New Dexterity: 8.";
        }
        
        public string CastSpell(int characterId, int spellId)
        {
            // For now, let's simulate casting a healing spell.
            return "Attempting to cast spell with ID: 1.\nHealing spell cast. Health increased by: 20. Mana decreased by spell's mana cost.";
        }
        
        public async Task<string> LatestEquipmentFeedback(int characterId)
        {
            var recentEquipment = (await GetEquipmentForCharacterAsync(characterId)).OrderByDescending(e => e.Id).FirstOrDefault();
            switch (recentEquipment?.Category.ToLower())
            {
                case "weapon":
                    return $"You've been awarded a powerful weapon, boosting your offensive capabilities.";
                case "armour":
                    return $"A new armor piece has been bestowed upon you, enhancing your defenses.";
                case "helmet":
                    return $"You've acquired a sturdy helmet, offering better protection.";
                case "boots":
                    return $"Swift boots have been granted, allowing for agile movements.";
                default:
                    return "You've acquired new equipment."; // Fallback message
            }
        }

        public async Task<string> LatestQuestFeedback(int characterId)
        {
            var recentQuest = (await GetQuestsForCharacterAsync(characterId)).OrderByDescending(q => q.Id).FirstOrDefault();
            switch (recentQuest?.Status.ToLower())
            {
                case "assigned":
                    return $"A new quest has been assigned to you. Prepare for the challenge!";
                case "complete":
                    return $"Congratulations! You've successfully completed your recent quest.";
                case "failed":
                    return $"The last quest posed great challenges and resulted in failure. Don't be disheartened, rise again!";
                default:
                    return "Your quest journey continues..."; // Fallback message
            }
        }
        
        public string CastSpellFeedback()
        {
            return "During combat, you unleashed a spell, channeling arcane energies against your foes.";
        }

        
        



    }
}