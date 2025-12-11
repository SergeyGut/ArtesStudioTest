using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IBoardView
{
    void CheckMisplacedGems();
}

public class BoardView : IBoardView, IInitializable
{
    private IGameBoard gameBoard;
    private ISettings settings;
    private ISpawnService spawnService;
    private IGemPool<IPiece> gemPool;
    private Dictionary<string, GameObject> unityObjects;
    
    private Transform GemsHolder => unityObjects["GemsHolder"].transform;

    public BoardView(
        Dictionary<string, GameObject> unityObjects,
        IGameBoard gameBoard,
        ISpawnService spawnService,
        ISettings settings,
        IGemPool<IPiece> gemPool)
    {
        this.unityObjects = unityObjects;
        this.gameBoard = gameBoard;
        this.settings = settings;
        this.spawnService = spawnService;
        this.gemPool = gemPool;
    }
    
    public void Initialize()
    {
        var parent = GemsHolder;
        
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
