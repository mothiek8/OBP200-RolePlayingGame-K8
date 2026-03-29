using System.Text;

namespace OBP200_RolePlayingGame;


class Program
{
    // ======= Globalt tillstånd  =======
    
    // Global variable that can store a player object
    static Player? Player;

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
            var choice = (Console.ReadLine() ?? "").Trim();

            if (choice == "1")
            {
                StartNewGame();
                RunGameLoop();
            }
            else if (choice == "2")
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
        var PlayerChoice = (Console.ReadLine() ?? "").Trim();

        switch (PlayerChoice)
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
            var room = Rooms[_currentRoomIndex];
            Console.WriteLine($"--- Rum {_currentRoomIndex + 1}/{Rooms.Count}: {room.Label} ({room.TypeName}) ---");

            bool continueAdventure = room.Enter();

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
            var post = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (post == "Q")
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

            var cmd = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (cmd == "A")
            {
                int damage = CalculatePlayerDamage(enemy.Defense);
                enemy.TakeDamage(damage);
                Console.WriteLine($"Du slog {enemy.Name} för {damage} skada.");
            }
            else if (cmd == "X")
            {
                int special = UseClassSpecial(enemy.Defense, isBoss);
                enemy.TakeDamage(special);
                Console.WriteLine($"Special! {enemy.Name} tar {special} skada.");
            }
            else if (cmd == "P")
            {
                UsePotion();
            }
            else if (cmd == "R" && !isBoss)
            {
                if (TryRunAway())
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
            int enemyDamage = CalculateEnemyDamage(enemy.Attack);
            ApplyDamageToPlayer(enemyDamage);
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
            var template = EnemyTemplates[Rng.Next(EnemyTemplates.Count)];

            return template.CreateEnemy(Rng);
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

    static int CalculatePlayerDamage(int enemyDef)
    {
        int atk = Player.Attack;
        string cls = Player.ClassType ?? "Warrior";

        // Beräkna grundskada
        int baseDmg = Math.Max(1, atk - (enemyDef / 2));
        int roll = Rng.Next(0, 3); // liten variation

        switch (cls.Trim())
        {
            case "Warrior":
                baseDmg += 1; // warrior buff
                break;
            case "Mage":
                baseDmg += 2; // mage buff
                break;
            case "Rogue":
                baseDmg += (Rng.NextDouble() < 0.2) ? 4 : 0; // rogue crit-chans
                break;
            default:
                baseDmg += 0;
                break;
        }

        return Math.Max(1, baseDmg + roll);
    }

    static int UseClassSpecial(int enemyDef, bool vsBoss)
    {
        string cls = Player.ClassType ?? "Warrior";
        int specialDmg = 0;

        // Hantering av specialförmågor
        if (cls == "Warrior")
        {
            // Heavy Strike: hög skada men självskada
            Console.WriteLine("Warrior använder Heavy Strike!");
            int atk = Player.Attack;
            specialDmg = Math.Max(2, atk + 3 - enemyDef);
            ApplyDamageToPlayer(2); // självskada
        }
        else if (cls == "Mage")
        {
            // Fireball: stor skada, kostar guld
            int gold = Player.Gold;
            if (gold >= 3)
            {
                Console.WriteLine("Mage kastar Fireball!");
                Player.Gold = gold - 3;
                int atk = Player.Attack;
                specialDmg = Math.Max(3, atk + 5 - (enemyDef / 2));
            }
            else
            {
                Console.WriteLine("Inte tillräckligt med guld för att kasta Fireball (kostar 3).");
                specialDmg = 0;
            }
        }
        else if (cls == "Rogue")
        {
            // Backstab: chans att ignorera försvar, hög risk/hög belöning
            if (Rng.NextDouble() < 0.5)
            {
                Console.WriteLine("Rogue utför en lyckad Backstab!");
                int atk = Player.Attack;
                specialDmg = Math.Max(4, atk + 6);
            }
            else
            {
                Console.WriteLine("Backstab misslyckades!");
                specialDmg = 1;
            }
        }
        else
        {
            specialDmg = 0;
        }

        // Dämpa skada mot bossen
        if (vsBoss)
        {
            specialDmg = (int)Math.Round(specialDmg * 0.8);
        }

        return Math.Max(0, specialDmg);
    }

    static int CalculateEnemyDamage(int enemyAtk)
    {
        int def = Player.Defense;
        int roll = Rng.Next(0, 3);

        int dmg = Math.Max(1, enemyAtk - (def / 2)) + roll;

        // Liten chans till "glancing blow" (minskad skada)
        if (Rng.NextDouble() < 0.1) dmg = Math.Max(1, dmg - 2);

        return dmg;
    }

    static void ApplyDamageToPlayer(int dmg)
    {
        Player.Health = Math.Max(0, Player.Health - Math.Max(0, dmg));
    }

    static void UsePotion()
    {
        if (Player.Potions <= 0)
        {
            Console.WriteLine("Du har inga drycker kvar.");
            return;
        }

        // Helning av spelaren
        int heal = 12;
        int oldHp = Player.Health;
        Player.Health = Math.Min(Player.MaxHealth, Player.Health + heal);
        Player.Potions--;

        Console.WriteLine($"Du dricker en dryck och återfår {Player.Health - oldHp} HP.");
    }

    static bool TryRunAway()
    {
        // Flyktschans baserad på karaktärsklass
        string cls = Player.ClassType ?? "Warrior";
        double chance = 0.25;
        if (cls == "Rogue") chance = 0.5;
        if (cls == "Mage") chance = 0.35;
        return Rng.NextDouble() < chance;
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
        int xp = Player.Experience;
        int lvl = Player.Level;
        int nextThreshold = lvl == 1 ? 10 : (lvl == 2 ? 25 : (lvl == 3 ? 45 : lvl * 20));

        if (xp >= nextThreshold)
        {
            Player.Level = lvl + 1;

            // Uppgradering baserad på karaktärsklass
            string cls = Player.ClassType ?? "Warrior";

            switch (cls)
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

    // ======= Room occurrences =======

    public static bool DoTreasure()
    {
        Console.WriteLine("Du hittar en gammal kista...");
        if (Rng.NextDouble() < 0.5)
        {
            int gold = Rng.Next(8, 15);
            AddPlayerGold(gold);
            Console.WriteLine($"Kistan innehåller {gold} guld!");
        }
        else
        {
            var items = new[] { "Iron Dagger", "Oak Staff", "Leather Vest", "Healing Herb" };
            string found = items[Rng.Next(items.Length)];
            Player.Inventory.Add(found);
            Console.WriteLine($"Du plockar upp: {found}");
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
            var val = (Console.ReadLine() ?? "").Trim();

            if (val == "1")
            {
                TryBuy(10, () => Player.Potions += 1, "Du köper en dryck.");
            }
            else if (val == "2")
            {
                TryBuy(25, () => Player.Attack += 2, "Du köper ett bättre vapen.");
            }
            else if (val == "3")
            {
                TryBuy(25, () => Player.Defense += 2, "Du köper bättre rustning.");
            }
            else if (val == "4")
            {
                SellMinorGems();
            }
            else if (val == "5")
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

    static void TryBuy(int cost, Action apply, string successMsg)
    {
        if (Player.Gold >= cost)
        {
            Player.Gold -= cost;
            apply();
            Console.WriteLine(successMsg);
        }
        else
        {
            Console.WriteLine("Du har inte råd.");
        }
    }

    static void SellMinorGems()
    {
        int count = Player.Inventory.Count(x => x == "Minor Gem");
        if (count == 0)
        {
            Console.WriteLine("Inga 'Minor Gem' i väskan.");
            return;
        }

        Player.Inventory.RemoveAll(x => x == "Minor Gem");

        AddPlayerGold(count * 5);
        Console.WriteLine($"Du säljer {count} st Minor Gem för {count * 5} guld.");
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

    // ======= Hjälpmetoder =======

    static int ParseInt(string s, int fallback)
    {
        try
        {
            int value = Convert.ToInt32(s);
            return value;
        }
        catch (Exception e)
        {
            return fallback;
        }
    }
}