
public interface ISpawnService
{
    IPiece SelectNonMatchingGem(GridPosition position);
    void SpawnGem(GridPosition position, IPiece gemToSpawn);
    void SpawnTopX(int x);
}

