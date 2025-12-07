using System.Collections.Generic;
using UnityEngine;

public class BombService
{
    private readonly IGameBoard gameBoard;
    private readonly IGameLogic gameLogic;
    private readonly IGemPool gemPool;
    private readonly SC_GameVariables settings;
    
    public BombService(IGameBoard gameBoard, IGameLogic gameLogic, IGemPool gemPool, SC_GameVariables settings)
    {
        this.gameLogic = gameLogic;
        this.gameBoard = gameBoard;
        this.gemPool = gemPool;
        this.settings = settings;
    }
    
    public void CreateBombs(Dictionary<Vector2Int, GlobalEnums.GemType> bombPositions, PooledHashSet<SC_Gem> newlyCreatedBombs)
    {
        foreach (var (pos, type) in bombPositions)
        {
            var bombPrefab = GetBombPrefabForType(type);
            var newBomb = gemPool.SpawnGem(bombPrefab, pos, gameLogic, gameBoard);
            newBomb.transform.position = new Vector3(pos.x, pos.y, 0);
            gameBoard.SetGem(pos, newBomb);
            newlyCreatedBombs.Value.Add(newBomb);
            
            gameBoard.Explosions.Remove(newBomb);
        }
    }
    
    private SC_Gem GetBombPrefabForType(GlobalEnums.GemType type)
    {
        foreach (SC_Gem bomb in settings.gemBombs)
        {
            if (bomb.type == type)
                return bomb;
        }
        return null;
    }
}

