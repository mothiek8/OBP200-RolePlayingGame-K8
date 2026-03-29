using System.Text;

namespace OBP200_RolePlayingGame;

class Program
{
    // ======= Global Status  =======

    // Global variable that can store a player object
    static Player Player;

    // Room: [type, label]
    // types: battle, treasure, shop, rest, boss
    static List<Room> Rooms = new List<Room>();

    // list for EnemyTemplates, coming from Character_Templates.cs. Index: [type, name, baseHealth, baseAttack, baseDefense, baseXpReward, baseGoldReward]
    static List<EnemyTemplate> EnemyTemplates = new List<EnemyTemplate>();

    // Map Status
    static int _currentRoomIndex = 0;

    // Random
    static Random Rng = new Random();

    // ======= Main =======

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
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

    static void ShowMainMenu()
    {
        Console.WriteLine("=== Text-RPG ===");
        Console.WriteLine("1. Nytt spel");
        Console.WriteLine("2. Avsluta");
    }

    static void StartNewGame()
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
                Player = PlayableClass.CreateWarrior(name);
                break;
            case "2":
                Player = PlayableClass.CreateMage(name);
                break;
            case "3":
                Player = PlayableClass.CreateRogue(name);
                break;
            default:
                Player = PlayableClass.CreateWarrior(name);
                break;
        }

        // Initiate Map (The Adventure is linear from point a to b)
        Rooms.Clear();
        Rooms.Add(new BattleRoom("Skogsstig"));
        Rooms.Add(new TreasureRoom("Gammal kista"));
        Rooms.Add(new ShopRoom("Vandrande köpman"));
        Rooms.Add(new BattleRoom("Grottans mynning"));
        Rooms.Add(new RestRoom("Lägereld"));
        Rooms.Add(new BattleRoom("Grottans djup"));
        Rooms.Add(new BossRoom("Urdraken"));

        _currentRoomIndex = 0;

        Console.WriteLine($"Välkommen, {Player.Name} the {Player.ClassType}!");
        ShowStatus();
    }

    static void RunGameLoop()
    {
        while (true)
        {
            var currentRoom = Rooms[_currentRoomIndex];
            Console.WriteLine($"--- Rum {_currentRoomIndex + 1}/{Rooms.Count}: {currentRoom.Label} ({currentRoom.TypeName}) ---");

            bool continueAdventure = currentRoom.Enter();

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

            _currentRoomIndex++;

            if (_currentRoomIndex >= Rooms.Count)
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

    public static bool DoBattle(bool isBoss)
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
                int damage = CombatSystem.CalculatePlayerDamage(Player, enemy, Rng);
                enemy.TakeDamage(damage);
                Console.WriteLine($"Du slog {enemy.Name} för {damage} skada.");
            }
            else if (command == "X")
            {
                int specialDamage = CombatSystem.UseClassSpecial(Player, enemy, isBoss, Rng);
                enemy.TakeDamage(specialDamage);
                Console.WriteLine($"Special! {enemy.Name} tar {specialDamage} skada.");
            }
            else if (command == "P")
            {
                CombatSystem.UsePotion(Player);
            }
            else if (command == "R" && !isBoss)
            {
                if (CombatSystem.TryRunAway(Player, Rng))
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
            int enemyDamage = CombatSystem.CalculateEnemyDamage(Player, enemy, Rng);
            CombatSystem.ApplyDamageToPlayer(Player, enemyDamage);
            Console.WriteLine($"{enemy.Name} anfaller och gör {enemyDamage} skada!");
        }

        if (IsPlayerDead())
        {
            return false; // avsluta äventyr
        }

        // Vinstrapporter, XP, guld, loot
        AddPlayerXp(enemy.XpReward);
        AddPlayerGold(enemy.GoldReward);

        Console.WriteLine($"Seger! +{enemy.XpReward} XP, +{enemy.GoldReward} guld.");
        MaybeDropLoot(enemy.Name);

        return true;
    }

    static Enemy GenerateEnemy(bool isBoss)
    {
        if (isBoss)
        {
            // Boss-mall
            return new Enemy("boss", "Urdraken", 55, 9, 4, 30, 50);
        }
        else
        {
            // Slumpa bland templates
            var enemyTemplate = EnemyTemplates[Rng.Next(EnemyTemplates.Count)];

            return enemyTemplate.CreateEnemy(Rng);
        }
    }

    static void InitEnemyTemplates()
    {
        EnemyTemplates.Clear();
        EnemyTemplates.Add(new EnemyTemplate("beast", "Vildsvin", 18, 4, 1, 6, 4));
        EnemyTemplates.Add(new EnemyTemplate("undead", "Skelett", 20, 5, 2, 7, 5));
        EnemyTemplates.Add(new EnemyTemplate("bandit", "Bandit", 16, 6, 1, 8, 6));
        EnemyTemplates.Add(new EnemyTemplate("slime", "Geléslem", 14, 3, 0, 5, 3));
    }

    static bool IsPlayerDead()
    {
        return Player.Health <= 0;
    }

    static void AddPlayerXp(int amount)
    {
        Player.Experience += Math.Max(0, amount);
        MaybeLevelUp();
    }

    static void AddPlayerGold(int amount)
    {
        Player.Gold += Math.Max(0, amount);
    }

    static void MaybeLevelUp()
    {
        // Nivåtrösklar
        int experience = Player.Experience;
        int level = Player.Level;
        int nextThreshold = level == 1 ? 10 : (level == 2 ? 25 : (level == 3 ? 45 : level * 20));

        if (experience >= nextThreshold)
        {
            Player.Level = level + 1;

            // Uppgradering baserad på karaktärsklass
            string playerClass = Player.ClassType ?? "Warrior";

            switch (playerClass)
            {
                case "Warrior":
                    Player.MaxHealth += 6;
                    Player.Attack += 2;
                    Player.Defense += 2;
                    break;
                case "Mage":
                    Player.MaxHealth += 4;
                    Player.Attack += 4;
                    Player.Defense += 1;
                    break;
                case "Rogue":
                    Player.MaxHealth += 5;
                    Player.Attack += 3;
                    Player.Defense += 1;
                    break;
                default:
                    Player.MaxHealth += 4;
                    Player.Attack += 3;
                    Player.Defense += 1;
                    break;
            }

            Player.Health = Player.MaxHealth; // full heal vid level up

            Console.WriteLine($"Du når nivå {Player.Level}! Värden ökade och HP återställd.");
        }
    }

    static void MaybeDropLoot(string enemyName)
    {
        // Enkel loot-regel
        if (Rng.NextDouble() < 0.35)
        {
            string item = "Minor Gem";
            if (enemyName.Contains("Urdraken")) item = "Dragon Scale";

            Player.Inventory.Add(item);

            Console.WriteLine($"Föremål hittat: {item} (lagt i din väska)");
        }
    }

    // ======= Room Events =======

    public static bool DoTreasure()
    {
        Console.WriteLine("Du hittar en gammal kista...");
        if (Rng.NextDouble() < 0.5)
        {
            int goldAmount = Rng.Next(8, 15);
            AddPlayerGold(goldAmount);
            Console.WriteLine($"Kistan innehåller {goldAmount} guld!");
        }
        else
        {
            var items = new[] { "Iron Dagger", "Oak Staff", "Leather Vest", "Healing Herb" };
            string foundItem = items[Rng.Next(items.Length)];
            Player.Inventory.Add(foundItem);
            Console.WriteLine($"Du plockar upp: {foundItem}");
        }

        return true;
    }

    public static bool DoShop()
    {
        Console.WriteLine("En vandrande köpman erbjuder sina varor:");
        while (true)
        {
            Console.WriteLine($"Guld: {Player.Gold} | Drycker: {Player.Potions}");
            Console.WriteLine("1) Köp dryck (10 guld)");
            Console.WriteLine("2) Köp vapen (+2 ATK) (25 guld)");
            Console.WriteLine("3) Köp rustning (+2 DEF) (25 guld)");
            Console.WriteLine("4) Sälj alla 'Minor Gem' (+5 guld/st)");
            Console.WriteLine("5) Lämna butiken");
            Console.Write("Val: ");
            var shopChoice = (Console.ReadLine() ?? "").Trim();

            if (shopChoice == "1")
            {
                TryBuy(10, () => Player.Potions += 1, "Du köper en dryck.");
            }
            else if (shopChoice == "2")
            {
                TryBuy(25, () => Player.Attack += 2, "Du köper ett bättre vapen.");
            }
            else if (shopChoice == "3")
            {
                TryBuy(25, () => Player.Defense += 2, "Du köper bättre rustning.");
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

    static void TryBuy(int cost, Action applyPurchase, string successMessage)
    {
        if (Player.Gold >= cost)
        {
            Player.Gold -= cost;
            applyPurchase();
            Console.WriteLine(successMessage);
        }
        else
        {
            Console.WriteLine("Du har inte råd.");
        }
    }

    static void SellMinorGems()
    {
        int minorGemCount = Player.Inventory.Count(item => item == "Minor Gem");
        if (minorGemCount == 0)
        {
            Console.WriteLine("Inga 'Minor Gem' i väskan.");
            return;
        }

        Player.Inventory.RemoveAll(item => item == "Minor Gem");

        AddPlayerGold(minorGemCount * 5);
        Console.WriteLine($"Du säljer {minorGemCount} st Minor Gem för {minorGemCount * 5} guld.");
    }

    public static bool DoRest()
    {
        Console.WriteLine("Du slår läger och vilar.");
        Player.Health = Player.MaxHealth;
        Console.WriteLine("HP återställt till max.");
        return true;
    }

    // ======= Status =======

    static void ShowStatus()
    {
        Console.WriteLine($"[{Player.Name} | {Player.ClassType}]  HP {Player.Health}/{Player.MaxHealth}  ATK {Player.Attack}  DEF {Player.Defense}  LVL {Player.Level}  XP {Player.Experience}  Guld {Player.Gold}  Drycker {Player.Potions}");
        if (Player.Inventory.Count > 0)
        {
            Console.WriteLine($"Väska: {string.Join(";", Player.Inventory)}");
        }
    }
}    