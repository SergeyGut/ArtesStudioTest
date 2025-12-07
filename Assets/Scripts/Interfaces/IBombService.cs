
using System.Collections.Generic;
using UnityEngine;

public interface IBombService
{
    void CreateBombs(Dictionary<Vector2Int, GlobalEnums.GemType> bombPositions, PooledHashSet<SC_Gem> newlyCreatedBombs);
}
