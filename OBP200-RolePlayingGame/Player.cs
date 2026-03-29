namespace OBP200_RolePlayingGame;

public class Player
{
    public string Name { get; set; }
    public string ClassType { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Gold { get; set; }
    public int Experience { get; set; }
    public int Level { get; set; }
    public int Potions { get; set; }
    public List<string> Inventory { get; set; }

    public Player(
        string name,
        string classType,
        int health,
        int maxHealth,
        int attack,
        int defense,
        int gold,
        int experience,
        int level,
        int potions,
        List<string> inventory)
    {
        Name = name;
        ClassType = classType;
        Health = health;
        MaxHealth = maxHealth;
        Attack = attack;
        Defense = defense;
        Gold = gold;
        Experience = experience;
        Level = level;
        Potions = potions;
        Inventory = inventory;
    }
}