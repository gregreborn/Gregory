namespace Progress_Brawlhalla.Models;

public class CharacterQuest
{
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public int QuestId { get; set; }
        public string Status { get; set; } // E.g., "complete"
        public int LevelWhenAssigned{ get; set; }
}