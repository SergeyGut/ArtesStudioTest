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
    
    public SC_Gem SelectNonMatchingGem(Vector2Int position)
    {
        using var validGems = PooledList<SC_Gem>.Get();
        
        for (int i = 0; i < settings.gems.Length; i++)
        {
            if (!gameBoard.MatchesAt(position, settings.gems[i]))
            {
                validGems.Value.Add(settings.gems[i]);
            }
        }
        
        if (validGems.Value.Count > 0)
        {
            return validGems.Value[Random.Range(0, validGems.Value.Count)];
        }
        
        return settings.gems[Random.Range(0, settings.gems.Length)];
    }
    
    public void SpawnGem(Vector2Int position, SC_Gem gemToSpawn, IGameLogic gameLogic)
    {
        if (Random.Range(0, 100f) < settings.bombChance)
            gemToSpawn = settings.bomb;

        SC_Gem gem = gemPool.SpawnGem(gemToSpawn, position, gameLogic, settings.dropHeight);
        gameBoard.SetGem(position.x, position.y, gem);
    }
    
    public void SpawnTopRow(IGameLogic gameLogic)
    {
        for (int x = 0; x < gameBoard.Width; x++)
        {
            int topY = gameBoard.Height - 1;
            SC_Gem topGem = gameBoard.GetGem(x, topY);
            
            if (topGem == null)
            {
                SC_Gem gemToSpawn = SelectNonMatchingGem(new Vector2Int(x, topY));
                SpawnGem(new Vector2Int(x, topY), gemToSpawn, gameLogic);
            }
        }
    }
}

