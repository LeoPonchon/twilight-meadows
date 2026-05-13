using System;

public enum WeatherType
{
    Sunny,
    Rainy,
    Stormy
}

public readonly struct GridPos : IEquatable<GridPos>
{
    public readonly int X;
    public readonly int Y;

    public GridPos(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(GridPos other) => X == other.X && Y == other.Y;
    public override bool Equals(object obj) => obj is GridPos other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"({X},{Y})";
}

public interface IDialogueUi
{
    bool IsDialogueOpen { get; }
    void CloseDialogue();
}

public interface IShopUi
{
    bool IsShopOpen { get; }
    void CloseShop();
}

public static class MovementRules
{
    public static float Speed(float baseSpeed, float sprintMultiplier, float walkMultiplier, bool sprinting, bool walking)
    {
        if (baseSpeed <= 0f) return 0f;
        if (sprinting) return baseSpeed * Math.Max(0f, sprintMultiplier);
        if (walking) return baseSpeed * Math.Max(0f, walkMultiplier);
        return baseSpeed;
    }
}
