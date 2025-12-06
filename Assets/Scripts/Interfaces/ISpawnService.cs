using UnityEngine;

public interface ISpawnService
{
    SC_Gem SelectNonMatchingGem(Vector2Int position);
    void SpawnGem(Vector2Int position, SC_Gem gemToSpawn, IGameLogic gameLogic);
    void SpawnTopRow(IGameLogic gameLogic);
}

