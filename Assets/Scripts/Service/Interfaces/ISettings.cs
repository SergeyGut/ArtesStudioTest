using System.Collections.Generic;

public interface ISettings : IBoardSettings
{
    IReadOnlyList<IPiece> Gems { get; }
    IReadOnlyList<IPiece> GemBombs { get; }
    float BombChance { get; }
    int DropHeight { get; }
    IPiece Bomb { get; }
    int MinMatchForBomb { get; }
    float ScoreSpeed { get; }
    float BombNeighborDelay { get; }
    float BombSelfDelay { get; }
    float BombPostSelfDelay { get; }
    float DecreaseRowDelay { get; }
    float DecreaseSingleRowDelay { get; }
    float DecreaseSingleColumnDelay { get; }
    float FindAllMatchesDelay { get; }
    float DestroyMatchesDelay { get; }
    float ChangeStateDelay { get; }
    object TilePrefabs { get; }
    float GemSpeed { get; }
    float GemSwapEase(float t);
    float GemDropSpeedEase(float t);
}