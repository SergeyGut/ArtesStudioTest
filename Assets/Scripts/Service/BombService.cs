using System.Collections.Generic;

public class BombService : IBombService
{
    private readonly IGameBoard gameBoard;
    private readonly IMatchService matchService;
    private readonly IGemPool<IPiece> gemPool;
    private readonly ISettings settings;
    
    public BombService(
        IGameBoard gameBoard, 
        IMatchService matchService, 
        IGemPool<IPiece> gemPool, 
        ISettings settings)
    {
        this.gameBoard = gameBoard;
        this.matchService = matchService;
        this.gemPool = gemPool;
        this.settings = settings;
    }
    
    public void CreateBombs(Dictionary<GridPosition, GemType> bombPositions, PooledHashSet<IPiece> newlyCreatedBombs)
    {
        foreach (var (pos, type) in bombPositions)
        {
            var bombPrefab = GetBombPrefabForType(type);
            var newBomb = gemPool.SpawnGem(bombPrefab, pos);
            gameBoard.SetGem(pos, newBomb);
            newlyCreatedBombs.Value.Add(newBomb);
            
            matchService.Explosions.Remove(newBomb);
        }
    }
    
    private IPiece GetBombPrefabForType(GemType type)
    {
        foreach (var bomb in settings.GemBombs)
        {
            if (bomb.Type == type)
                return bomb;
        }
        return null;
    }
}

