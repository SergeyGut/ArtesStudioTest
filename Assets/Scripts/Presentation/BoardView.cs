using UnityEngine;
using Zenject;

public class BoardView : IBoardView, IInitializable
{
    private IGameBoard gameBoard;
    private ISettings settings;
    private ISpawnService spawnService;
    private IGemPool<IPiece> gemPool;
    private Transform gemsHolder;

    public BoardView(
        [Inject(Id = "GemsHolder")] Transform gemsHolder,
        IGameBoard gameBoard,
        ISpawnService spawnService,
        ISettings settings,
        IGemPool<IPiece> gemPool)
    {
        this.gemsHolder = gemsHolder;
        this.gameBoard = gameBoard;
        this.settings = settings;
        this.spawnService = spawnService;
        this.gemPool = gemPool;
    }
    
    public void Initialize()
    {
        var parent = gemsHolder;
        
        for (int x = 0; x < gameBoard.Width; x++)
        for (int y = 0; y < gameBoard.Height; y++)
        {
            Vector2 _pos = new Vector2(x, y);
            GameObject _bgTile = Object.Instantiate(settings.TilePrefabs as GameObject, _pos, Quaternion.identity);
            _bgTile.transform.SetParent(parent);
            _bgTile.name = "BG Tile - " + x + ", " + y;

            var gemToSpawn = spawnService.SelectNonMatchingGem(new GridPosition(x, y));
            spawnService.SpawnGem(new GridPosition(x, y), gemToSpawn);
        }
    }
    
    public void CheckMisplacedGems()
    {
        using var foundGems = PooledHashSet<IPiece>.Get();
        foundGems.Value.UnionWith(Object.FindObjectsOfType<SC_Gem>());
        
        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                IPiece curGem = gameBoard.GetGem(x, y);
                if (curGem != null)
                {
                    foundGems.Value.Remove(curGem);
                }
            }
        }

        foreach (var g in foundGems.Value)
            gemPool.ReturnGem(g);
    }
}
