
namespace Domain.Interfaces
{
    public interface IGemData
    {
        GemType Type { get; }
        IPieceView PieceView { get; }
        bool IsColorBomb { get; }
        int BlastSize { get; }
        int ScoreValue { get; }
    }
}