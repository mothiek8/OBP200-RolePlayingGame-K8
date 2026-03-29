namespace OBP200_RolePlayingGame;

// The playable classes the player can choose. The values in the constructor for player are located in Player.cs
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