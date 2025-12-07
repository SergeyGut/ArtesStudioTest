using UnityEngine;

public class MatchService : IMatchService
{
    private readonly IGameBoard gameBoard;
    private readonly SC_GameVariables settings;
    
    public MatchService(IGameBoard gameBoard, SC_GameVariables settings)
    {
        this.gameBoard = gameBoard;
        this.settings = settings;
    }
    
    public PooledDictionary<Vector2Int, GlobalEnums.GemType> CollectBombCreationPositions()
    {
        var bombCreationPositions = PooledDictionary<Vector2Int, GlobalEnums.GemType>.Get();
        
        foreach (var matchInfo in gameBoard.MatchInfoMap)
        {
            if (matchInfo.MatchedGems.Count >= settings.minMatchForBomb)
            {
                SC_Gem firstGem = null;
                foreach (var gem in matchInfo.MatchedGems)
                {
                    firstGem = gem;
                    break;
                }
                if (firstGem != null)
                {
                    bombCreationPositions.Value.TryAdd(matchInfo.UserActionPos ?? firstGem.posIndex, firstGem.type);
                }
            }
        }
        
        return bombCreationPositions;
    }
    
    public void CollectAndDestroyMatchedGems(IDestroyService destroyService)
    {
        using var matchedGems = PooledList<SC_Gem>.Get();
        foreach (var matchInfo in gameBoard.MatchInfoMap)
        {
            foreach (var gem in matchInfo.MatchedGems)
            {
                if (gem && !gem.isColorBomb && gem.type != GlobalEnums.GemType.bomb)
                {
                    matchedGems.Value.Add(gem);
                }
            }
        }
        destroyService.DestroyGems(matchedGems.Value);
    }
    
    public PooledList<SC_Gem> CollectNonBombExplosions(PooledHashSet<SC_Gem> newlyCreatedBombs)
    {
        var nonBombExplosions = PooledList<SC_Gem>.Get();
        foreach (var gem in gameBoard.Explosions)
        {
            if (gem && !gem.isColorBomb && gem.type != GlobalEnums.GemType.bomb && !newlyCreatedBombs.Value.Contains(gem))
            {
                nonBombExplosions.Value.Add(gem);
            }
        }
        return nonBombExplosions;
    }
    
    public PooledList<SC_Gem> CollectBombExplosions(PooledHashSet<SC_Gem> newlyCreatedBombs)
    {
        var bombExplosions = PooledList<SC_Gem>.Get();
        foreach (var gem in gameBoard.Explosions)
        {
            if (gem && (gem.isColorBomb || gem.type == GlobalEnums.GemType.bomb) && !newlyCreatedBombs.Value.Contains(gem))
            {
                bombExplosions.Value.Add(gem);
            }
        }
        return bombExplosions;
    }
}

