namespace OBP200_RolePlayingGame;
// Polymorhic Class Room, several ineritors
public abstract class Room
{
    public string Label { get; }
    public abstract string TypeName { get; }

    protected Room(string label)
    {
        Label = label;
    }

    public abstract bool Enter();
}

public class BattleRoom : Room
{
    public BattleRoom(string label) : base(label) { }
    public override string TypeName => "battle";

    public override bool Enter()
    {
        return Program.DoBattle(false);
    }
}

public class BossRoom : Room
{
    public BossRoom(string label) : base(label) { }
    public override string TypeName => "boss";

    public override bool Enter()
    {
        return Program.DoBattle(true);
    }
}

public class TreasureRoom : Room
{
    public TreasureRoom(string label) : base(label) { }
    public override string TypeName => "treasure";

    public override bool Enter()
    {
        return Program.DoTreasure();
    }
}

public class ShopRoom : Room
{
    public ShopRoom(string label) : base(label) { }
    public override string TypeName => "shop";

    public override bool Enter()
    {
        return Program.DoShop();
    }
}

public class RestRoom : Room
{
    public RestRoom(string label) : base(label) { }
    public override string TypeName => "rest";

    public override bool Enter()
    {
        return Program.DoRest();
    }
}