using System.Collections.Generic;
using UnityEngine;

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
    
    public void DestroyGems(IEnumerable<IPiece> gems)
    {
        foreach (var gem in gems)
        {
            if (gem != null)
            {
                scoreService.AddScore(gem.ScoreValue);
                DestroyMatchedGemsAt(gem);
            }
        }
    }
    
    private void DestroyMatchedGemsAt(IPiece gem)
    {
        if (gem != null)
        {
            var position = gem.Position;
            gem.RunDestroyEffect();
            gemPool.ReturnGem(gem as SC_Gem);
            
            if (gameBoard.GetGem(position) == gem)
            {
                gameBoard.SetGem(position, null);
            }
        }
    }
}

