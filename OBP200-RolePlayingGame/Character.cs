namespace OBP200_RolePlayingGame;
// Inherited by class Player located in Player.cs and class Enemy located in 

public abstract class Character
{
    public string Name { get; protected set; }
    public int Health { get; protected set; }
    public int MaxHealth { get; protected set; }
    public int Attack { get; protected set; }
    public int Defense { get; protected set; }

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