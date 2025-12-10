using System.Collections.Generic;
using UnityEngine;

public interface IBombService
{
    void CreateBombs(Dictionary<GridPosition, GemType> bombPositions, PooledHashSet<IPiece> newlyCreatedBombs);
}
