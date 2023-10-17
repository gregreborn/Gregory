using System;

namespace Progress_Brawlhalla.Models;

public class CharacterSpell
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public int SpellId { get; set; }
    public string Slot { get; set; } // E.g., "Heal"
    public DateTime AcquiredAt{ get; set; }
    public bool IsActive { get; set; }
}