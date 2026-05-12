using System;

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

