namespace Progress_Brawlhalla.Models
{
    public class Character
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public string Class { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Money { get; set; }
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Mp { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int MaxMp { get; set; } 
    }
}