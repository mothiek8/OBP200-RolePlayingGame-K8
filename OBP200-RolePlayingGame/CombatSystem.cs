namespace OBP200_RolePlayingGame;

// Combat System
public static class CombatSystem
{
    // Base Danage, basic attack
    public static int CalculatePlayerDamage(Player player, Enemy enemy, Random random)
    {
        int playerAttack = player.Attack;
        string playerClass = player.ClassType ?? "Warrior";

        int baseDamage = Math.Max(1, playerAttack - (enemy.Defense / 2));
        int randomRoll = random.Next(0, 3);

        switch (playerClass.Trim())
        {
            case "Warrior":
                baseDamage += 1;
                break;
            case "Mage":
                baseDamage += 2;
                break;
            case "Rogue":
                baseDamage += (random.NextDouble() < 0.2) ? 4 : 0;
                break;
            default:
                baseDamage += 0;
                break;
        }

        return Math.Max(1, baseDamage + randomRoll);
    }
    
    // Special Attack
    public static int UseClassSpecial(Player player, Enemy enemy, bool versusBoss, Random random)
    {
        string playerClass = player.ClassType ?? "Warrior";
        int specialDamage = 0;

        if (playerClass == "Warrior")
        {
            Console.WriteLine("Warrior använder Heavy Strike!");
            int playerAttack = player.Attack;
            specialDamage = Math.Max(2, playerAttack + 3 - enemy.Defense);
            ApplyDamageToPlayer(player, 2);
        }
        else if (playerClass == "Mage")
        {
            int playerGold = player.Gold;
            if (playerGold >= 3)
            {
                Console.WriteLine("Mage kastar Fireball!");
                player.Gold = playerGold - 3;
                int playerAttack = player.Attack;
                specialDamage = Math.Max(3, playerAttack + 5 - (enemy.Defense / 2));
            }
            else
            {
                Console.WriteLine("Inte tillräckligt med guld för att kasta Fireball (kostar 3).");
                specialDamage = 0;
            }
        }
        else if (playerClass == "Rogue")
        {
            if (random.NextDouble() < 0.5)
            {
                Console.WriteLine("Rogue utför en lyckad Backstab!");
                int playerAttack = player.Attack;
                specialDamage = Math.Max(4, playerAttack + 6);
            }
            else
            {
                Console.WriteLine("Backstab misslyckades!");
                specialDamage = 1;
            }
        }
        else
        {
            specialDamage = 0;
        }

        if (versusBoss)
        {
            specialDamage = (int)Math.Round(specialDamage * 0.8);
        }

        return Math.Max(0, specialDamage);
    }
    
    // Enemy Damage
    public static int CalculateEnemyDamage(Player player, Enemy enemy, Random random)
    {
        int playerDefense = player.Defense;
        int randomRoll = random.Next(0, 3);

        int damage = Math.Max(1, enemy.Attack - (playerDefense / 2)) + randomRoll;

        if (random.NextDouble() < 0.1)
        {
            damage = Math.Max(1, damage - 2);
        }

        return damage;
    }

    public static void ApplyDamageToPlayer(Player player, int damage)
    {
        player.Health = Math.Max(0, player.Health - Math.Max(0, damage));
    }
    
    // Potions
    public static void UsePotion(Player player)
    {
        if (player.Potions <= 0)
        {
            Console.WriteLine("Du har inga drycker kvar.");
            return;
        }

        int healAmount = 12;
        int previousHealth = player.Health;
        player.Health = Math.Min(player.MaxHealth, player.Health + healAmount);
        player.Potions--;

        Console.WriteLine($"Du dricker en dryck och återfår {player.Health - previousHealth} HP.");
    }
    
    // Run Away
    public static bool TryRunAway(Player player, Random random)
    {
        string playerClass = player.ClassType ?? "Warrior";
        double escapeChance = 0.25;

        if (playerClass == "Rogue") escapeChance = 0.5;
        if (playerClass == "Mage") escapeChance = 0.35;

        return random.NextDouble() < escapeChance;
    }
}