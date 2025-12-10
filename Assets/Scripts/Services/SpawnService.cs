using UnityEngine;

public class SpawnService : ISpawnService
{
    private readonly IGameBoard gameBoard;
    private readonly IGemPool gemPool;
    private readonly SC_GameVariables settings;
    
    public SpawnService(IGameBoard gameBoard, IGemPool gemPool, SC_GameVariables settings)
    {
        this.gameBoard = gameBoard;
        this.gemPool = gemPool;
        this.settings = settings;
    }
    
    public SC_Gem SelectNonMatchingGem(GridPosition position)
    {
        using var validGems = PooledList<SC_Gem>.Get();
        using var matchCounts = PooledList<int>.Get();
        int lowestMatchCount = int.MaxValue;
        
        for (int i = 0; i < settings.gems.Length; i++)
        {
            int matchCount = gameBoard.GetMatchCountAt(position, settings.gems[i]);
            matchCounts.Value.Add(matchCount);
            
            if (matchCount == 0)
            {
                validGems.Value.Add(settings.gems[i]);
            }
            else if (matchCount < lowestMatchCount)
            {
                lowestMatchCount = matchCount;
            }
        }
        
        if (validGems.Value.Count > 0)
        {
            return validGems.Value[Random.Range(0, validGems.Value.Count)];
        }
        
        validGems.Value.Clear();
        for (int i = 0; i < settings.gems.Length; i++)
        {
            if (matchCounts.Value[i] == lowestMatchCount)
            {
                validGems.Value.Add(settings.gems[i]);
            }
        }
        
        return validGems.Value[Random.Range(0, validGems.Value.Count)];
    }

    public void SpawnGem(GridPosition position, SC_Gem gemToSpawn, IGameLogic gameLogic, IGameBoard gameBoard)
    {
        if (Random.Range(0, 100f) < settings.bombChance)
            gemToSpawn = settings.bomb;

        SC_Gem gem = gemPool.SpawnGem(gemToSpawn, position, gameLogic, gameBoard, settings.dropHeight);
        gameBoard.SetGem(position, gem);
    }
    
    public void SpawnTopX(int x, IGameLogic gameLogic, IGameBoard gameBoard)
    {
        int topY = gameBoard.Height - 1;
        IPiece topGem = gameBoard.GetGem(x, topY);
        
        if (topGem == null)
        {
            SC_Gem gemToSpawn = SelectNonMatchingGem(new GridPosition(x, topY));
            SpawnGem(new GridPosition(x, topY), gemToSpawn, gameLogic, gameBoard);
        }
    }
}

