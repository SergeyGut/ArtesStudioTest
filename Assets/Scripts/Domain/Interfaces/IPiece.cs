
namespace Domain.Interfaces
{
    public interface IPiece
    {
        GemType Type { get; }
        ref GridPosition Position { get; }
        ref GridPosition PrevPosition { get; }
        bool IsColorBomb { get; }
        int BlastSize { get; }
        bool IsMatch { get; set; }
        bool IsSwapMovement { get; set; }
        bool IsMoving { get; set; }
        bool JustSpawned { get; set; }
        int ScoreValue { get; }
    }    
}

