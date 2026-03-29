namespace OBP200_RolePlayingGame;
// Inherited by class Player located in Player.cs and class Enemy located in Enemy.cs

public abstract class Character
{
    public string Name { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }

    public bool IsDead => Health <= 0;

    protected Character(string name, int health, int maxHealth, int attack, int defense)
    {
        Name = name;
        Health = health;
        MaxHealth = maxHealth;
        Attack = attack;
        Defense = defense;
    }

    public void TakeDamage(int amount)
    {
        Health = Math.Max(0, Health - Math.Max(0, amount));
    }

    public int Heal(int amount)
    {
        int before = Health;
        Health = Math.Min(MaxHealth, Health + Math.Max(0, amount));
        return Health - before;
    }

    public void RestoreToFull()
    {
        Health = MaxHealth;
    }
}