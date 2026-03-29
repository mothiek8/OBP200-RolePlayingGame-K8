namespace OBP200_RolePlayingGame;

// Polymorphic class Room, several inheritors
public abstract class Room
{
    public string Label { get; }
    public abstract string TypeName { get; }

    protected Room(string label)
    {
        Label = label;
    }

    public abstract bool Enter(Game game);
}

public class BattleRoom : Room
{
    public BattleRoom(string label) : base(label) { }
    public override string TypeName => "battle";

    public override bool Enter(Game game)
    {
        return game.DoBattle(false);
    }
}

public class BossRoom : Room
{
    public BossRoom(string label) : base(label) { }
    public override string TypeName => "boss";

    public override bool Enter(Game game)
    {
        return game.DoBattle(true);
    }
}

public class TreasureRoom : Room
{
    public TreasureRoom(string label) : base(label) { }
    public override string TypeName => "treasure";

    public override bool Enter(Game game)
    {
        return game.DoTreasure();
    }
}

public class ShopRoom : Room
{
    public ShopRoom(string label) : base(label) { }
    public override string TypeName => "shop";

    public override bool Enter(Game game)
    {
        return game.DoShop();
    }
}

public class RestRoom : Room
{
    public RestRoom(string label) : base(label) { }
    public override string TypeName => "rest";

    public override bool Enter(Game game)
    {
        return game.DoRest();
    }
}