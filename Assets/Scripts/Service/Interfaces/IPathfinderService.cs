
using Domain;
using Domain.Interfaces;
using Domain.Pool;

namespace Service.Interfaces
{
    public interface IPathfinderService
    {
        PooledDictionary<GridPosition, GemType> CollectBombCreationPositions();
        PooledHashSet<IPiece> CollectMatchedGems();
        PooledList<IPiece> CollectExplosionsNonBomb(PooledHashSet<IPiece> matchedGems);
        PooledList<IPiece> CollectExplosionsBomb(PooledHashSet<IPiece> matchedGems);
    }
}