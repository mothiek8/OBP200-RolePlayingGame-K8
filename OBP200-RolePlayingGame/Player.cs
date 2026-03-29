namespace OBP200_RolePlayingGame;

// polymorphic class inheriting from Character.cs
public class Player : Character
{
    public string ClassType { get; set; }
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
        : base(name, health, maxHealth, attack, defense)
    {
        ClassType = classType;
        Gold = gold;
        Experience = experience;
        Level = level;
        Potions = potions;
        Inventory = inventory;
    }
}