using System;

public class SpawnService : ISpawnService
{
    private readonly IGameBoard gameBoard;
    private readonly IMatchCounterService matchCounterService;
    private readonly IGemPool<IPiece> gemPool;
    private readonly ISettings settings;
    
    private readonly Random random = new();
    
    public SpawnService(
        IGameBoard gameBoard,
        IMatchCounterService matchCounterService,
        IGemPool<IPiece> gemPool,
        ISettings settings)
    {
        this.gameBoard = gameBoard;
        this.matchCounterService = matchCounterService;
        this.gemPool = gemPool;
        this.settings = settings;
    }
    
    public IPiece SelectNonMatchingGem(GridPosition position)
    {
        using var validGems = PooledList<IPiece>.Get();
        using var matchCounts = PooledList<int>.Get();
        int lowestMatchCount = int.MaxValue;
        var gems = settings.Gems;
        
        for (int i = 0; i < gems.Count; i++)
        {
            int matchCount = matchCounterService.GetMatchCountAt(position, gems[i]);
            matchCounts.Value.Add(matchCount);
            
            if (matchCount == 0)
            {
                validGems.Value.Add(gems[i]);
            }
            else if (matchCount < lowestMatchCount)
            {
                lowestMatchCount = matchCount;
            }
        }
        
        if (validGems.Value.Count > 0)
        {
            return validGems.Value[random.Next(0, validGems.Value.Count)];
        }
        
        validGems.Value.Clear();
        for (int i = 0; i < gems.Count; i++)
        {
            if (matchCounts.Value[i] == lowestMatchCount)
            {
                validGems.Value.Add(gems[i]);
            }
        }
        
        return validGems.Value[random.Next(0, validGems.Value.Count)];
    }

    public void SpawnGem(GridPosition position, IPiece gemToSpawn)
    {
        if (random.Next(0, 100) < settings.BombChance)
            gemToSpawn = settings.Bomb;

        var gem = gemPool.SpawnGem(gemToSpawn, position, settings.DropHeight);
        gameBoard.SetGem(position, gem);
    }
    
    public void SpawnTopX(int x)
    {
        int topY = gameBoard.Height - 1;
        IPiece topGem = gameBoard.GetGem(x, topY);
        
        if (topGem == null)
        {
            var gemToSpawn = SelectNonMatchingGem(new GridPosition(x, topY));
            SpawnGem(new GridPosition(x, topY), gemToSpawn);
        }
    }
}

