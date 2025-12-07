using UnityEngine;

public interface IMatchService
{
    PooledDictionary<Vector2Int, GlobalEnums.GemType> CollectBombCreationPositions();
    void CollectAndDestroyMatchedGems(IDestroyService destroyService);
    PooledList<SC_Gem> CollectNonBombExplosions(PooledHashSet<SC_Gem> newlyCreatedBombs);
    PooledList<SC_Gem> CollectBombExplosions(PooledHashSet<SC_Gem> newlyCreatedBombs);
}
