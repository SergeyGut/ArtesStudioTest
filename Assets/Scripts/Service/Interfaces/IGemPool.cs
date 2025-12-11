
public interface IGemPool<T> where T : IPiece
{
    int AvailableCount { get; }
    int ActiveCount { get; }

    T SpawnGem(IPiece item, GridPosition position, float dropHeight = 0f);
    void ReturnGem(T item);
    void ClearPool();
}

