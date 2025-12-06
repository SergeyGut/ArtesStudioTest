using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SC_GameLogic : MonoBehaviour
{
    private Dictionary<string, GameObject> unityObjects;
    private int score = 0;
    private float displayScore = 0;
    private GameBoard gameBoard;
    private GlobalEnums.GameState currentState = GlobalEnums.GameState.move;
    public GlobalEnums.GameState CurrentState => currentState;
    private GemPool gemPool;

    #region MonoBehaviour
    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        displayScore = Mathf.Lerp(displayScore, gameBoard.Score, Settings.scoreSpeed * Time.deltaTime);
        unityObjects["Txt_Score"].GetComponent<TMPro.TextMeshProUGUI>().text = displayScore.ToString("0");
    }
    #endregion

    #region Logic
    private SC_GameVariables Settings
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => SC_GameVariables.Instance;
    }
    private void Init()
    {
        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _obj = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _obj)
            unityObjects.Add(g.name,g);

        gameBoard = new GameBoard(7, 7);
        gemPool = new GemPool(unityObjects["GemsHolder"].transform);
        Setup();
    }
    private void Setup()
    {
        for (int x = 0; x < gameBoard.Width; x++)
            for (int y = 0; y < gameBoard.Height; y++)
            {
                Vector2 _pos = new Vector2(x, y);
                GameObject _bgTile = Instantiate(Settings.bgTilePrefabs, _pos, Quaternion.identity);
                _bgTile.transform.SetParent(unityObjects["GemsHolder"].transform);
                _bgTile.name = "BG Tile - " + x + ", " + y;

                SC_Gem gemToSpawn = SelectNonMatchingGem(new Vector2Int(x, y));
                SpawnGem(new Vector2Int(x, y), gemToSpawn);
            }
    }
    public void StartGame()
    {
        unityObjects["Txt_Score"].GetComponent<TextMeshProUGUI>().text = score.ToString("0");
    }
    private SC_Gem SelectNonMatchingGem(Vector2Int _Position)
    {
        int gemToUse = Random.Range(0, Settings.gems.Length);
        int iterations = 0;
        int maxIterations = 100;

        while (gameBoard.MatchesAt(_Position, Settings.gems[gemToUse]) && iterations < maxIterations)
        {
            gemToUse = Random.Range(0, Settings.gems.Length);
            iterations++;
        }

        return Settings.gems[gemToUse];
    }

    private void SpawnGem(Vector2Int _Position, SC_Gem _GemToSpawn)
    {
        if (Random.Range(0, 100f) < Settings.bombChance)
            _GemToSpawn = Settings.bomb;

        SC_Gem _gem = gemPool.SpawnGem(_GemToSpawn, _Position, this, Settings.dropHeight);
        gameBoard.SetGem(_Position.x,_Position.y, _gem);
    }
    public void SetGem(int _X,int _Y, SC_Gem _Gem)
    {
        gameBoard.SetGem(_X,_Y, _Gem);
    }
    public SC_Gem GetGem(int _X, int _Y)
    {
        return gameBoard.GetGem(_X, _Y);
    }
    public void SetState(GlobalEnums.GameState _CurrentState)
    {
        currentState = _CurrentState;
    }
    public void DestroyMatches()
    {
        StartCoroutine(DestroyMatchesCo());
    }

    private IEnumerator DestroyMatchesCo()
    {
        using var bombCreationPositions = CollectBombCreationPositions();
        using var newlyCreatedBombs = PooledHashSet<SC_Gem>.Get();
        
        CollectAndDestroyMatchedGems();
        CreateBombs(bombCreationPositions, newlyCreatedBombs);
        
        yield return DestroyExplosionsWithDelay(newlyCreatedBombs);
        
        yield return DecreaseRowCo();
    }

    private PooledDictionary<Vector2Int, GlobalEnums.GemType> CollectBombCreationPositions()
    {
        var bombCreationPositions = PooledDictionary<Vector2Int, GlobalEnums.GemType>.Get();
        
        foreach (var matchInfo in gameBoard.MatchInfoMap)
        {
            if (matchInfo.MatchedGems.Count >= Settings.minMatchForBomb)
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

    private void CollectAndDestroyMatchedGems()
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
        DestroyGems(matchedGems.Value);
    }

    private IEnumerator DestroyExplosionsWithDelay(PooledHashSet<SC_Gem> newlyCreatedBombs)
    {
        using var nonBombExplosions = CollectNonBombExplosions(newlyCreatedBombs);
        if (nonBombExplosions.Value.Count > 0)
        {
            yield return WaitForSecondsPool.Get(Settings.bombNeighborDelay);
            DestroyGems(nonBombExplosions.Value);
        }
        
        using var bombExplosions = CollectBombExplosions(newlyCreatedBombs);
        if (bombExplosions.Value.Count > 0)
        {
            yield return WaitForSecondsPool.Get(Settings.bombSelfDelay);
            DestroyGems(bombExplosions.Value);
        }
    }

    private PooledList<SC_Gem> CollectNonBombExplosions(PooledHashSet<SC_Gem> newlyCreatedBombs)
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

    private PooledList<SC_Gem> CollectBombExplosions(PooledHashSet<SC_Gem> newlyCreatedBombs)
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

    private void DestroyGems(IEnumerable<SC_Gem> gems)
    {
        foreach (var gem in gems)
        {
            if (gem != null)
            {
                ScoreCheck(gem);
                DestroyMatchedGemsAt(gem.posIndex);
            }
        }
    }

    private void CreateBombs(Dictionary<Vector2Int, GlobalEnums.GemType> bombPositions, PooledHashSet<SC_Gem> newlyCreatedBombs)
    {
        foreach (var (pos, type) in bombPositions)
        {
            var bombPrefab = GetBombPrefabForType(type);
            var newBomb = gemPool.SpawnGem(bombPrefab, pos, this, 0);
            newBomb.transform.position = new Vector3(pos.x, pos.y, 0);
            gameBoard.SetGem(pos.x, pos.y, newBomb);
            newlyCreatedBombs.Value.Add(newBomb);
            
            gameBoard.Explosions.Remove(newBomb);
        }
    }

    private SC_Gem GetBombPrefabForType(GlobalEnums.GemType type)
    {
        foreach (SC_Gem bomb in Settings.gemBombs)
        {
            if (bomb.type == type)
                return bomb;
        }
        return null;
    }
    private IEnumerator DecreaseRowCo()
    {
        yield return WaitForSecondsPool.Get(Settings.decreaseRowDelay);

        bool hasActivity = true;
        while (hasActivity)
        {
            SpawnTopRow();
            hasActivity = DropSingleRow();

            if (hasActivity)
            {
                yield return WaitForSecondsPool.Get(Settings.decreaseSingleRowDelay);
            }
        }

        CheckMisplacedGems();
        yield return WaitForSecondsPool.Get(Settings.findAllMatchesDelay);
        gameBoard.FindAllMatches();
        if (gameBoard.MatchInfoMap.Count > 0)
        {
            yield return WaitForSecondsPool.Get(Settings.destroyMatchesDelay);
            DestroyMatches();
        }
        else
        {
            yield return WaitForSecondsPool.Get(Settings.changeStateDelay);
            currentState = GlobalEnums.GameState.move;
        }
    }

    private void SpawnTopRow()
    {
        for (int x = 0; x < gameBoard.Width; x++)
        {
            int topY = gameBoard.Height - 1;
            SC_Gem topGem = gameBoard.GetGem(x, topY);
            
            if (topGem == null)
            {
                SC_Gem gemToSpawn = SelectNonMatchingGem(new Vector2Int(x, topY));
                SpawnGem(new Vector2Int(x, topY), gemToSpawn);
            }
        }
    }

    private bool DropSingleRow()
    {
        bool anyDropped = false;

        for (int y = 1; y < gameBoard.Height; y++)
        {
            for (int x = 0; x < gameBoard.Width; x++)
            {
                SC_Gem currentGem = gameBoard.GetGem(x, y);
                SC_Gem gemBelow = gameBoard.GetGem(x, y - 1);

                if (currentGem != null && gemBelow == null)
                {
                    currentGem.posIndex.y--;
                    SetGem(x, y - 1, currentGem);
                    SetGem(x, y, null);
                    anyDropped = true;
                }
            }

            if (anyDropped)
            {
                return true;
            }
        }

        return false;
    }

    public void ScoreCheck(SC_Gem gemToCheck)
    {
        gameBoard.Score += gemToCheck.scoreValue;
    }
    private void DestroyMatchedGemsAt(Vector2Int _Pos)
    {
        SC_Gem _curGem = gameBoard.GetGem(_Pos.x,_Pos.y);
        if (_curGem != null)
        {
            Instantiate(_curGem.destroyEffect, new Vector2(_Pos.x, _Pos.y), Quaternion.identity);

            gemPool.ReturnGem(_curGem);
            SetGem(_Pos.x,_Pos.y, null);
        }
    }

    private void CheckMisplacedGems()
    {
        using var foundGems = PooledList<SC_Gem>.Get();
        foundGems.Value.AddRange(FindObjectsOfType<SC_Gem>());
        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                SC_Gem _curGem = gameBoard.GetGem(x, y);
                if (foundGems.Value.Contains(_curGem))
                    foundGems.Value.Remove(_curGem);
            }
        }

        foreach (SC_Gem g in foundGems.Value)
            gemPool.ReturnGem(g);
    }
    public void FindAllMatches(Vector2Int? posIndex = null, Vector2Int? otherPosIndex = null)
    {
        if (posIndex.HasValue)
            gameBoard.FindAllMatches(posIndex);
        else if (otherPosIndex.HasValue)
            gameBoard.FindAllMatches(otherPosIndex);
        else
            gameBoard.FindAllMatches();
    }

    #endregion
}
