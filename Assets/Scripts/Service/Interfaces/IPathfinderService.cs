
using Domain;
using Domain.Interfaces;
using Domain.Pool;

namespace Service.Interfaces
{
    public interface IPathfinderService
    {
        PooledDictionary<GridPosition, GemType> CollectBombCreationPositions();
        PooledList<IPiece> CollectMatchedGems();
        PooledList<IPiece> CollectNonBombExplosions(PooledHashSet<IPiece> newlyCreatedBombs);
        PooledList<IPiece> CollectBombExplosions(PooledHashSet<IPiece> newlyCreatedBombs);
    }
}