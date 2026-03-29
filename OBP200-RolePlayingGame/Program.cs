using System.Text;

namespace OBP200_RolePlayingGame;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Game game = new Game();
        game.Run();
    }
}

public class Game
{
    // ======= Global Status  =======

    // Global variable that can store a player object
    private Player player;

    
    // Room list, types: battle, treasure, shop, rest, boss
    private readonly List<Room> rooms = new List<Room>();

    // list for EnemyTemplates, coming from Character_Templates.cs. Index: [type, name, baseHealth, baseAttack, baseDefense, baseXpReward, baseGoldReward]
    private readonly List<EnemyTemplate> enemyTemplates = new List<EnemyTemplate>();

    // Map Status
    private int currentRoomIndex = 0;

    // Random
    private readonly Random random = new Random();

    public Player Player => player;
    public Random Random => random;

    // ======= Main =======

    public void Run()
    {
        InitEnemyTemplates();

        while (true)
        {
            ShowMainMenu();
            Console.Write("Välj: ");
            var menuChoice = (Console.ReadLine() ?? "").Trim();

            if (menuChoice == "1")
            {
                StartNewGame();
                RunGameLoop();
            }
            else if (menuChoice == "2")
            {
                Console.WriteLine("Avslutar...");
                return;
            }
            else
            {
                Console.WriteLine("Ogiltigt val.");
            }

            Console.WriteLine();
        }
    }

    // ======= Meny & Init =======

    private void ShowMainMenu()
    {
        Console.WriteLine("=== Text-RPG ===");
        Console.WriteLine("1. Nytt spel");
        Console.WriteLine("2. Avsluta");
    }

    private void StartNewGame()
    {
        Console.Write("Ange namn: ");
        var name = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "Namnlös";

        Console.WriteLine("Välj klass: 1) Warrior  2) Mage  3) Rogue");
        Console.Write("Val: ");
        var playerChoice = (Console.ReadLine() ?? "").Trim();

        switch (playerChoice)
        {
            case "1":
                player = PlayableClass.CreateWarrior(name);
                break;
            case "2":
                player = PlayableClass.CreateMage(name);
                break;
            case "3":
                player = PlayableClass.CreateRogue(name);
                break;
            default:
                player = PlayableClass.CreateWarrior(name);
                break;
        }

        // Initiate Map (The Adventure is linear from point a to b)
        rooms.Clear();
        rooms.Add(new BattleRoom("Skogsstig"));
        rooms.Add(new TreasureRoom("Gammal kista"));
        rooms.Add(new ShopRoom("Vandrande köpman"));
        rooms.Add(new BattleRoom("Grottans mynning"));
        rooms.Add(new RestRoom("Lägereld"));
        rooms.Add(new BattleRoom("Grottans djup"));
        rooms.Add(new BossRoom("Urdraken"));

        currentRoomIndex = 0;

        Console.WriteLine($"Välkommen, {player.Name} the {player.ClassType}!");
        ShowStatus();
    }

    private void RunGameLoop()
    {
        while (true)
        {
            var currentRoom = rooms[currentRoomIndex];
            Console.WriteLine($"--- Rum {currentRoomIndex + 1}/{rooms.Count}: {currentRoom.Label} ({currentRoom.TypeName}) ---");

            bool continueAdventure = currentRoom.Enter(this);

            if (IsPlayerDead())
            {
                Console.WriteLine("Du har stupat... Spelet över.");
                break;
            }

            if (!continueAdventure)
            {
                Console.WriteLine("Du lämnar äventyret för nu.");
                break;
            }

            currentRoomIndex++;

            if (currentRoomIndex >= rooms.Count)
            {
                Console.WriteLine();
                Console.WriteLine("Du har klarat äventyret!");
                break;
            }

            Console.WriteLine();
            Console.WriteLine("[C] Fortsätt     [Q] Avsluta till huvudmeny");
            Console.Write("Val: ");
            var continueChoice = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (continueChoice == "Q")
            {
                Console.WriteLine("Tillbaka till huvudmenyn.");
                break;
            }

            Console.WriteLine();
        }
    }

    // ======= Battle =======

    public bool DoBattle(bool isBoss)
    {
        var enemy = GenerateEnemy(isBoss);
        Console.WriteLine($"En {enemy.Name} dyker upp! (HP {enemy.Health}, ATK {enemy.Attack}, DEF {enemy.Defense})");

        while (enemy.Health > 0 && !IsPlayerDead())
        {
            Console.WriteLine();
            ShowStatus();
            Console.WriteLine($"Fiende: {enemy.Name} HP={enemy.Health}");
            Console.WriteLine("[A] Attack   [X] Special   [P] Dryck   [R] Fly");
            if (isBoss) Console.WriteLine("(Du kan inte fly från en boss!)");
            Console.Write("Val: ");

            var command = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (command == "A")
            {
                int damage = CombatSystem.CalculatePlayerDamage(player, enemy, random);
                enemy.TakeDamage(damage);
                Console.WriteLine($"Du slog {enemy.Name} för {damage} skada.");
            }
            else if (command == "X")
            {
                int specialDamage = CombatSystem.UseClassSpecial(player, enemy, isBoss, random);
                enemy.TakeDamage(specialDamage);
                Console.WriteLine($"Special! {enemy.Name} tar {specialDamage} skada.");
            }
            else if (command == "P")
            {
                CombatSystem.UsePotion(player);
            }
            else if (command == "R" && !isBoss)
            {
                if (CombatSystem.TryRunAway(player, random))
                {
                    Console.WriteLine("Du flydde!");
                    return true; // fortsätt äventyr
                }
                else
                {
                    Console.WriteLine("Misslyckad flykt!");
                }
            }
            else
            {
                Console.WriteLine("Du tvekar...");
            }

            if (enemy.Health <= 0) break;

            // Fiendens tur
            int enemyDamage = CombatSystem.CalculateEnemyDamage(player, enemy, random);
            CombatSystem.ApplyDamageToPlayer(player, enemyDamage);
            Console.WriteLine($"{enemy.Name} anfaller och gör {enemyDamage} skada!");
        }

        if (IsPlayerDead())
        {
            return false; // avsluta äventyr
        }

        // Victory rewards, XP, gold, loot
        AddPlayerXp(enemy.XpReward);
        AddPlayerGold(enemy.GoldReward);

        Console.WriteLine($"Seger! +{enemy.XpReward} XP, +{enemy.GoldReward} guld.");
        MaybeDropLoot(enemy.Name);

        return true;
    }

    private Enemy GenerateEnemy(bool isBoss)
    {
        if (isBoss)
        {
            // Boss template
            return new Enemy("boss", "Urdraken", 55, 9, 4, 30, 50);
        }
        else
        {
            // Pick a random template
            var enemyTemplate = enemyTemplates[random.Next(enemyTemplates.Count)];
            return enemyTemplate.CreateEnemy(random);
        }
    }

    private void InitEnemyTemplates()
    {
        enemyTemplates.Clear();
        enemyTemplates.Add(new EnemyTemplate("beast", "Vildsvin", 18, 4, 1, 6, 4));
        enemyTemplates.Add(new EnemyTemplate("undead", "Skelett", 20, 5, 2, 7, 5));
        enemyTemplates.Add(new EnemyTemplate("bandit", "Bandit", 16, 6, 1, 8, 6));
        enemyTemplates.Add(new EnemyTemplate("slime", "Geléslem", 14, 3, 0, 5, 3));
    }

    private bool IsPlayerDead()
    {
        return player.Health <= 0;
    }

    private void AddPlayerXp(int amount)
    {
        player.Experience += Math.Max(0, amount);
        MaybeLevelUp();
    }

    private void AddPlayerGold(int amount)
    {
        player.Gold += Math.Max(0, amount);
    }

    private void MaybeLevelUp()
    {
        // Level thresholds
        int experience = player.Experience;
        int level = player.Level;
        int nextThreshold = level == 1 ? 10 : (level == 2 ? 25 : (level == 3 ? 45 : level * 20));

        if (experience >= nextThreshold)
        {
            player.Level = level + 1;

            // Upgrade based on player class
            string playerClass = player.ClassType ?? "Warrior";

            switch (playerClass)
            {
                case "Warrior":
                    player.MaxHealth += 6;
                    player.Attack += 2;
                    player.Defense += 2;
                    break;
                case "Mage":
                    player.MaxHealth += 4;
                    player.Attack += 4;
                    player.Defense += 1;
                    break;
                case "Rogue":
                    player.MaxHealth += 5;
                    player.Attack += 3;
                    player.Defense += 1;
                    break;
                default:
                    player.MaxHealth += 4;
                    player.Attack += 3;
                    player.Defense += 1;
                    break;
            }

            player.Health = player.MaxHealth; // full heal vid level up

            Console.WriteLine($"Du når nivå {player.Level}! Värden ökade och HP återställd.");
        }
    }

    private void MaybeDropLoot(string enemyName)
    {
        // Simple loot rule with randomization
        if (random.NextDouble() < 0.35)
        {
            string item = "Minor Gem";
            if (enemyName.Contains("Urdraken")) item = "Dragon Scale";

            player.Inventory.Add(item);

            Console.WriteLine($"Föremål hittat: {item} (lagt i din väska)");
        }
    }

    // ======= Room Events =======

    public bool DoTreasure()
    {
        Console.WriteLine("Du hittar en gammal kista...");
        if (random.NextDouble() < 0.5)
        {
            int goldAmount = random.Next(8, 15);
            AddPlayerGold(goldAmount);
            Console.WriteLine($"Kistan innehåller {goldAmount} guld!");
        }
        else
        {
            var items = new[] { "Iron Dagger", "Oak Staff", "Leather Vest", "Healing Herb" };
            string foundItem = items[random.Next(items.Length)];
            player.Inventory.Add(foundItem);
            Console.WriteLine($"Du plockar upp: {foundItem}");
        }

        return true;
    }

    public bool DoShop()
    {
        Console.WriteLine("En vandrande köpman erbjuder sina varor:");
        while (true)
        {
            Console.WriteLine($"Guld: {player.Gold} | Drycker: {player.Potions}");
            Console.WriteLine("1) Köp dryck (10 guld)");
            Console.WriteLine("2) Köp vapen (+2 ATK) (25 guld)");
            Console.WriteLine("3) Köp rustning (+2 DEF) (25 guld)");
            Console.WriteLine("4) Sälj alla 'Minor Gem' (+5 guld/st)");
            Console.WriteLine("5) Lämna butiken");
            Console.Write("Val: ");
            var shopChoice = (Console.ReadLine() ?? "").Trim();

            if (shopChoice == "1")
            {
                TryBuy(10, () => player.Potions += 1, "Du köper en dryck.");
            }
            else if (shopChoice == "2")
            {
                TryBuy(25, () => player.Attack += 2, "Du köper ett bättre vapen.");
            }
            else if (shopChoice == "3")
            {
                TryBuy(25, () => player.Defense += 2, "Du köper bättre rustning.");
            }
            else if (shopChoice == "4")
            {
                SellMinorGems();
            }
            else if (shopChoice == "5")
            {
                Console.WriteLine("Du säger adjö till köpmannen.");
                break;
            }
            else
            {
                Console.WriteLine("Köpmannen förstår inte ditt val.");
            }
        }

        return true;
    }

    private void TryBuy(int cost, Action applyPurchase, string successMessage)
    {
        if (player.Gold >= cost)
        {
            player.Gold -= cost;
            applyPurchase();
            Console.WriteLine(successMessage);
        }
        else
        {
            Console.WriteLine("Du har inte råd.");
        }
    }

    private void SellMinorGems()
    {
        int minorGemCount = player.Inventory.Count(item => item == "Minor Gem");
        if (minorGemCount == 0)
        {
            Console.WriteLine("Inga 'Minor Gem' i väskan.");
            return;
        }

        player.Inventory.RemoveAll(item => item == "Minor Gem");

        AddPlayerGold(minorGemCount * 5);
        Console.WriteLine($"Du säljer {minorGemCount} st Minor Gem för {minorGemCount * 5} guld.");
    }

    public bool DoRest()
    {
        Console.WriteLine("Du slår läger och vilar.");
        player.Health = player.MaxHealth;
        Console.WriteLine("HP återställt till max.");
        return true;
    }

    // ======= Status =======

    private void ShowStatus()
    {
        Console.WriteLine($"[{player.Name} | {player.ClassType}]  HP {player.Health}/{player.MaxHealth}  ATK {player.Attack}  DEF {player.Defense}  LVL {player.Level}  XP {player.Experience}  Guld {player.Gold}  Drycker {player.Potions}");
        if (player.Inventory.Count > 0)
        {
            Console.WriteLine($"Väska: {string.Join(";", player.Inventory)}");
        }
    }
}    