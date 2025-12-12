using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface IGemData
    {
        GemType Type { get; }
        IPiece GemViewPrefab { get; }
    }
}