namespace Progress_Brawlhalla.Models;

public class CharacterEquipment
{
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public int EquipmentId { get; set; }
        public string Slot { get; set; }
        public string Category{ get; set; }
}

