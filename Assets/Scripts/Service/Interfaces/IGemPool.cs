
public interface IGemPool<T> where T : IPiece
{
    int AvailableCount { get; }
    int ActiveCount { get; }
    
    T SpawnGem(T item, GridPosition position, IGameLogic gameLogic, IGameBoard gameBoard, float dropHeight = 0f);
    void ReturnGem(T item);
    void ClearPool();
}

