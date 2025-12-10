using System.Collections.Generic;

public interface ISettings
{
    IReadOnlyList<IPiece> Gems { get; }
    IReadOnlyList<IPiece> GemBombs { get; }
    float BombChance { get; }
    int DropHeight { get; }
    IPiece Bomb { get; }
    int MinMatchForBomb { get; }
}