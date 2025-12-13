
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
        bool IsSwap { get; set; }
        bool IsMoving { get; set; }
        int ScoreValue { get; }
    }    
}

