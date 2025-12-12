
namespace Domain.Interfaces
{
    public interface IGemData
    {
        GemType Type { get; }
        IPiece GemView { get; }
        bool IsColorBomb { get; }
        int BlastSize { get; }
        int ScoreValue { get; }
    }
}