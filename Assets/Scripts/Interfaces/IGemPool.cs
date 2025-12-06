using UnityEngine;

public interface IGemPool
{
    int AvailableCount { get; }
    int ActiveCount { get; }
    
    SC_Gem SpawnGem(SC_Gem prefab, Vector2Int position, IGameLogic gameLogic, float dropHeight = 0f);
    void ReturnGem(SC_Gem gem);
    void ClearPool();
}

