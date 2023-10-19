using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Npgsql;
using Progress_Brawlhalla.Config;
using Progress_Brawlhalla.Models;

namespace Progress_Brawlhalla.Services
{
    public class CharacterService
    {
        private readonly string ConnectionString;
        public CharacterService()
        {
            ConnectionString = DatabaseConfig.ConnectionString;

        }

        public List<Character> GetAllCharacters()
        {
            List<Character> characters = new List<Character>();

            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand("SELECT id, name, class, level, experience, hp, mp, strength, dexterity, money, max_hp, max_mp FROM characters", connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                characters.Add(new Character
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Class = reader.GetString(2),
                    Level = reader.GetInt32(3),
                    Experience = reader.GetInt32(4),
                    Hp = reader.GetInt32(5),
                    Mp = reader.GetInt32(6),
                    Strength = reader.GetInt32(7),
                    Dexterity = reader.GetInt32(8),
                    Money = reader.GetInt32(9),
                    MaxHp = reader.GetInt32(10),
                    MaxMp = reader.GetInt32(11)
                });

            }

            return characters;
        }
        
        public void AddCharacter(Character character)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            int hp = 10, maxHp = 10, mp = 10, maxMp = 10, strength = 10, dexterity = 10;

            switch (character.Class)
            {
                case "Gold Knight":
                    strength = 10;
                    dexterity = 10;
                    hp = 15;
                    maxHp = 15;
                    mp = 10;
                    maxMp = 10;
                    break;
                case "Speedster":
                    strength = 10;
                    dexterity = 15;
                    hp = 10;
                    maxHp = 10;
                    mp = 10;
                    maxMp = 10;
                    break;
                case "Mage":
                    strength = 10;
                    dexterity = 10;
                    hp = 10;
                    maxHp = 10;
                    mp = 15;
                    maxMp = 15;
                    break;
                case "Org":
                    strength = 15;
                    dexterity = 10;
                    hp = 10;
                    maxHp = 10;
                    mp = 15;
                    maxMp = 15;
                    break;
            }

            using var cmd = new NpgsqlCommand(@"INSERT INTO characters(name, class, level, experience, hp, mp, strength, dexterity, money, max_hp, max_mp) VALUES (@Name, @Class, 1, 0, @Hp, @Mp, @Strength, @Dexterity, 500, @MaxHp, @MaxMp) RETURNING id;", connection);
    
            cmd.Parameters.AddWithValue("Name", character.Name);
            cmd.Parameters.AddWithValue("Class", character.Class);
            cmd.Parameters.AddWithValue("Hp", hp);
            cmd.Parameters.AddWithValue("Mp", mp);
            cmd.Parameters.AddWithValue("Strength", strength);
            cmd.Parameters.AddWithValue("Dexterity", dexterity);
            cmd.Parameters.AddWithValue("MaxHp", maxHp);
            cmd.Parameters.AddWithValue("MaxMp", maxMp);

            character.Id = (int)cmd.ExecuteScalar();
        }


        
        public List<CharacterEquipment> GetCharacterEquipmentsById(int characterId)
        {
            var characterEquipments = new List<CharacterEquipment>();

            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand("SELECT id, character_id, equipment_id, slot, category FROM character_equipment WHERE character_id = @CharacterId", connection);
            cmd.Parameters.AddWithValue("CharacterId", characterId);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var characterEquipment = new CharacterEquipment
                {
                    Id = reader.GetInt32(0),
                    CharacterId = reader.GetInt32(1),
                    EquipmentId = reader.GetInt32(2),
                    Slot = reader.GetString(3),
                    Category = reader.GetString(4)
                };
                characterEquipments.Add(characterEquipment);
            }

            return characterEquipments;
        }




        public Character GetCharacterById(int characterId)
        {
            Character character = null;

            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand("SELECT id, name, class, level, experience, hp, mp, strength, dexterity, money, max_hp, max_mp FROM characters WHERE id = @CharacterId", connection);
            cmd.Parameters.AddWithValue("CharacterId", characterId);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                character = new Character
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Class = reader.GetString(2),
                    Level = reader.GetInt32(3),
                    Experience = reader.GetInt32(4),
                    Hp = reader.GetInt32(5),
                    Mp = reader.GetInt32(6),
                    Strength = reader.GetInt32(7),
                    Dexterity = reader.GetInt32(8),
                    Money = reader.GetInt32(9),
                    MaxHp = reader.GetInt32(10),
                    MaxMp = reader.GetInt32(11)
                };
            }

            return character;
        }

        public List<CharacterQuest> GetQuestsByCharacterId(int characterId)
        {
            List<CharacterQuest> characterQuests = new List<CharacterQuest>();

            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand("SELECT id, character_id, quest_id, status, level_when_assigned FROM character_quests WHERE character_id = @CharacterId", connection);
            cmd.Parameters.AddWithValue("CharacterId", characterId);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                characterQuests.Add(new CharacterQuest
                {
                    Id = reader.GetInt32(0),
                    CharacterId = reader.GetInt32(1),
                    QuestId = reader.GetInt32(2),
                    Status = reader.GetString(3),
                    LevelWhenAssigned = reader.GetInt32(4)
                });
            }

            return characterQuests;        }
    }
}