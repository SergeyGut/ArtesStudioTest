
public interface ISpawnService
{
    SC_Gem SelectNonMatchingGem(GridPosition position);
    void SpawnGem(GridPosition position, SC_Gem gemToSpawn, IGameLogic gameLogic, IGameBoard gameBoard);
    void SpawnTopX(int x, IGameLogic gameLogic, IGameBoard gameBoard);
}

