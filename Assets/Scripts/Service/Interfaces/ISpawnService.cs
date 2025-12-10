
public interface ISpawnService
{
    IPiece SelectNonMatchingGem(GridPosition position);
    void SpawnGem(GridPosition position, IPiece gemToSpawn, IGameLogic gameLogic, IGameBoard gameBoard);
    void SpawnTopX(int x, IGameLogic gameLogic, IGameBoard gameBoard);
}

