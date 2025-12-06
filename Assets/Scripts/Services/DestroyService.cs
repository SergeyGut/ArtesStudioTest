using System.Collections.Generic;
using UnityEngine;

public interface IDestroyService
{
    void DestroyGems(IEnumerable<SC_Gem> gems);
    void DestroyMatchedGemsAt(Vector2Int position);
}

public class DestroyService : IDestroyService
{
    private readonly IGameBoard gameBoard;
    private readonly IGemPool gemPool;
    private readonly IScoreService scoreService;
    
    public DestroyService(IGameBoard gameBoard, IGemPool gemPool, IScoreService scoreService)
    {
        this.gameBoard = gameBoard;
        this.gemPool = gemPool;
        this.scoreService = scoreService;
    }
    
    public void DestroyGems(IEnumerable<SC_Gem> gems)
    {
        foreach (var gem in gems)
        {
            if (gem != null)
            {
                scoreService.AddScore(gem.scoreValue);
                DestroyMatchedGemsAt(gem.posIndex);
            }
        }
    }
    
    public void DestroyMatchedGemsAt(Vector2Int position)
    {
        SC_Gem gem = gameBoard.GetGem(position.x, position.y);
        if (gem != null)
        {
            Object.Instantiate(gem.destroyEffect, new Vector2(position.x, position.y), Quaternion.identity);
            gemPool.ReturnGem(gem);
            gameBoard.SetGem(position.x, position.y, null);
        }
    }
}

