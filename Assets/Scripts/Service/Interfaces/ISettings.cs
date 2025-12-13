using System.Collections.Generic;
using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface ISettings : IBoardSettings
    {
        IReadOnlyList<IPieceData> Pieces { get; }
        IReadOnlyList<IPieceData> PieceBombs { get; }
        float BombChance { get; }
        int DropHeight { get; }
        IPieceData Bomb { get; }
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
        float PieceSpeed { get; }
        float PieceSwapEase(float t);
        float PieceDropSpeedEase(float t);
    }
}