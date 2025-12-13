
namespace Domain.Interfaces
{
    public interface IPieceData
    {
        PieceType Type { get; }
        IPieceView PieceView { get; }
        bool IsColorBomb { get; }
        int BlastSize { get; }
        int ScoreValue { get; }
    }
}