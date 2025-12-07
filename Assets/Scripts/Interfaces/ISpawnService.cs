using UnityEngine;

public interface ISpawnService
{
    SC_Gem SelectNonMatchingGem(Vector2Int position);
    void SpawnGem(Vector2Int position, SC_Gem gemToSpawn, IGameLogic gameLogic, IGameBoard gameBoard);
    void SpawnTopRow(IGameLogic gameLogic, IGameBoard gameBoard);
}

