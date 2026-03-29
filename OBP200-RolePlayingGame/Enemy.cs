namespace OBP200_RolePlayingGame;
// polymorphic class inheriting from Character.cs
public class Enemy : Character
{
    public string Type { get; }
    public int XpReward { get; }
    public int GoldReward { get; }

    public Enemy(string type, string name, int health, int attack, int defense, int xpReward, int goldReward)
        : base(name, health, health, attack, defense)
    {
        Type = type;
        XpReward = xpReward;
        GoldReward = goldReward;
    }
}

