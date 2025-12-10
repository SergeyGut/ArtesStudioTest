using System;

public struct GridPosition : IEquatable<GridPosition>
{
    public static readonly GridPosition zero = new(0, 0);

    public int X;
    public int Y;

    public GridPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj)
    {
        return obj is GridPosition other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(GridPosition left, GridPosition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GridPosition left, GridPosition right)
    {
        return !(left == right);
    }

    public bool Equals(GridPosition other)
    {
        return X == other.X && Y == other.Y;
    }
}