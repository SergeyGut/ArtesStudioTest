
public interface IGemPool
{
    int AvailableCount { get; }
    int ActiveCount { get; }
    
    SC_Gem SpawnGem(SC_Gem prefab, GridPosition position, IGameLogic gameLogic, IGameBoard gameBoard, float dropHeight = 0f);
    void ReturnGem(SC_Gem gem);
    void ClearPool();
}

