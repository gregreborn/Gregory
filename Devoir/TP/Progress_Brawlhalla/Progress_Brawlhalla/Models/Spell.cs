namespace Progress_Brawlhalla.Models
{
    public class Spell
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string EffectType { get; set; } // E.g., "Heal"
        public int EffectValue { get; set; }
        public int ManaCost { get; set; }
    }
}