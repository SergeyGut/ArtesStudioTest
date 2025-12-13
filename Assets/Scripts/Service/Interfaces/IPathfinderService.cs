
using Domain;
using Domain.Interfaces;
using Domain.Pool;

namespace Service.Interfaces
{
    public interface IPathfinderService
    {
        PooledDictionary<GridPosition, PieceType> CollectBombCreationPositions();
        PooledHashSet<IPiece> CollectMatchedPieces();
        PooledList<IPiece> CollectExplosionsNonBomb(PooledHashSet<IPiece> matchedPieces);
        PooledList<IPiece> CollectExplosionsBomb(PooledHashSet<IPiece> matchedPieces);
    }
}