namespace Progress_Brawlhalla.Models;

public class Quest
{
    public int Id { get; set; }
    public string Description { get; set; }
    public int RewardMoney { get; set; }
    public int RewardExperience { get; set; }
    public int? EquipmentRewardId { get; set; }  // Nullable, in case there's no equipment reward
}

