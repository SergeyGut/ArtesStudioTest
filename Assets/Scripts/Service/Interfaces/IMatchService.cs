
public interface IMatchService
{
    PooledDictionary<GridPosition, GemType> CollectBombCreationPositions();
    void CollectAndDestroyMatchedGems(IDestroyService destroyService);
    PooledList<IPiece> CollectNonBombExplosions(PooledHashSet<IPiece> newlyCreatedBombs);
    PooledList<IPiece> CollectBombExplosions(PooledHashSet<IPiece> newlyCreatedBombs);
}
