
namespace Domain.Interfaces
{
    public interface IReadOnlyPiece
    {
        GridPosition Position { get; }
        bool IsSwap { get; }
        bool IsMoving { get; }
    }
    
    public interface IPiece : IReadOnlyPiece
    {
        GemType Type { get; }
        new ref GridPosition Position { get; }
        GridPosition PrevPosition { get; set; }
        bool IsColorBomb { get; }
        int BlastSize { get; }
        bool IsMatch { get; set; }
        new bool IsSwap { set; }
        new bool IsMoving { get; set; }
        int ScoreValue { get; }
    }
}

