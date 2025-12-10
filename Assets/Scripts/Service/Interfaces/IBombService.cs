using System.Collections.Generic;

public interface IBombService
{
    void CreateBombs(Dictionary<GridPosition, GemType> bombPositions, PooledHashSet<IPiece> newlyCreatedBombs);
}
