
public class PathfinderService : IPathfinderService
{
    private readonly IMatchService matchService;
    private readonly ISettings settings;
    
    public PathfinderService(IMatchService matchService, ISettings settings)
    {
        this.matchService = matchService;
        this.settings = settings;
    }
    
    public PooledDictionary<GridPosition, GemType> CollectBombCreationPositions()
    {
        var bombCreationPositions = PooledDictionary<GridPosition, GemType>.Get();
        
        foreach (var matchInfo in matchService.MatchInfoMap)
        {
            if (matchInfo.MatchedGems.Count >= settings.MinMatchForBomb)
            {
                IPiece firstGem = null;
                foreach (var gem in matchInfo.MatchedGems)
                {
                    firstGem = gem;
                    break;
                }
                if (firstGem != null)
                {
                    bombCreationPositions.Value.TryAdd(matchInfo.UserActionPos ?? firstGem.Position, firstGem.Type);
                }
            }
        }
        
        return bombCreationPositions;
    }
    
    public PooledList<IPiece> CollectMatchedGems()
    {
        var matchedGems = PooledList<IPiece>.Get();
        foreach (var matchInfo in matchService.MatchInfoMap)
        {
            foreach (var gem in matchInfo.MatchedGems)
            {
                if (gem != null && !gem.IsColorBomb && gem.Type != GemType.bomb)
                {
                    matchedGems.Value.Add(gem);
                }
            }
        }
        
        return matchedGems;
    }
    
    public PooledList<IPiece> CollectNonBombExplosions(PooledHashSet<IPiece> newlyCreatedBombs)
    {
        var nonBombExplosions = PooledList<IPiece>.Get();
        foreach (var gem in matchService.Explosions)
        {
            if (gem != null && !gem.IsColorBomb && gem.Type != GemType.bomb && !newlyCreatedBombs.Value.Contains(gem))
            {
                nonBombExplosions.Value.Add(gem);
            }
        }
        return nonBombExplosions;
    }
    
    public PooledList<IPiece> CollectBombExplosions(PooledHashSet<IPiece> newlyCreatedBombs)
    {
        var bombExplosions = PooledList<IPiece>.Get();
        foreach (var gem in matchService.Explosions)
        {
            if (gem != null && (gem.IsColorBomb || gem.Type == GemType.bomb) && !newlyCreatedBombs.Value.Contains(gem))
            {
                bombExplosions.Value.Add(gem);
            }
        }
        return bombExplosions;
    }
}

