namespace OBP200_RolePlayingGame;

// Templates for the playable classes the player can choose. The values in the constructor for player are located in Player.cs
// Index: name, classType (PlayableClass), health, maxHealth, attack, defense, gold, experience, level, potions, and an inventory (using List)
public static class PlayableClass
{
    public static Player CreateWarrior(string name) =>
        new Player(name, "Warrior", 40, 40, 7, 5, 15, 0, 1, 2,
            new List<string> { "Wooden Sword", "Cloth Armor" });

    public static Player CreateMage(string name) =>
        new Player(name, "Mage", 28, 28, 10, 2, 15, 0, 1, 2,
            new List<string> { "Wooden Sword", "Cloth Armor" });

    public static Player CreateRogue(string name) =>
        new Player(name, "Rogue", 32, 32, 8, 3, 20, 0, 1, 3,
            new List<string> { "Wooden Sword", "Cloth Armor" });
}

// Templates for different enemy types i:e bosses, base enemies et cetera.
public class EnemyTemplate
{
    public string Type { get; }
    public string Name { get; }
    public int BaseHealth { get; }
    public int BaseAttack { get; }
    public int BaseDefense { get; }
    public int BaseXpReward { get; }
    public int BaseGoldReward { get; }

    public EnemyTemplate(string type, string name, int baseHealth, int baseAttack, int baseDefense, int baseXpReward, int baseGoldReward)
    {
        Type = type;
        Name = name;
        BaseHealth = baseHealth;
        BaseAttack = baseAttack;
        BaseDefense = baseDefense;
        BaseXpReward = baseXpReward;
        BaseGoldReward = baseGoldReward;
    }

    public Enemy CreateEnemy(Random random)
    {
        int health = BaseHealth + random.Next(-1, 3);
        int attack = BaseAttack + random.Next(0, 2);
        int defense = BaseDefense + random.Next(0, 2);
        int xpReward = BaseXpReward + random.Next(0, 3);
        int goldReward = BaseGoldReward + random.Next(0, 3);

        return new Enemy(Type, Name, health, attack, defense, xpReward, goldReward);
    }
}