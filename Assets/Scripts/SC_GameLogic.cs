using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SC_GameLogic : MonoBehaviour, IGameLogic
{
    private Dictionary<string, GameObject> unityObjects;
    private float displayScore = 0;
    private IGameBoard gameBoard;
    private GlobalEnums.GameState currentState = GlobalEnums.GameState.move;
    public GlobalEnums.GameState CurrentState => currentState;
    private IGemPool gemPool;
    private TMPro.TextMeshProUGUI scoreText;
    private float scoreSpeed;
    private int lastDisplayedScoreInt = -1;
    
    private MatchService matchService;
    private SpawnService spawnService;
    private DestroyService destroyService;
    private ScoreService scoreService;
    private BombService bombService;
    private BoardService boardService;

    #region MonoBehaviour
    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        scoreText = unityObjects["Txt_Score"].GetComponent<TMPro.TextMeshProUGUI>();
        scoreSpeed = Settings.scoreSpeed;
        StartGame();
    }

    private void Update()
    {
        displayScore = Mathf.Lerp(displayScore, scoreService.Score, scoreSpeed * Time.deltaTime);
        
        int currentScoreInt = Mathf.RoundToInt(displayScore);
        if (currentScoreInt != lastDisplayedScoreInt)
        {
            scoreText.text = currentScoreInt.ToString();
            lastDisplayedScoreInt = currentScoreInt;
        }
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
        
        scoreService = new ScoreService(gameBoard);
        destroyService = new DestroyService(gameBoard, gemPool, scoreService);
        matchService = new MatchService(gameBoard, Settings);
        spawnService = new SpawnService(gameBoard, gemPool, Settings);
        bombService = new BombService(gameBoard, gemPool, Settings);
        boardService = new BoardService(gameBoard, spawnService);
        
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

                SC_Gem gemToSpawn = spawnService.SelectNonMatchingGem(new Vector2Int(x, y));
                spawnService.SpawnGem(new Vector2Int(x, y), gemToSpawn, this);
            }
    }
    
    public void StartGame()
    {
        if (scoreText != null)
        {
            scoreText.text = scoreService.Score.ToString();
        }
        displayScore = scoreService.Score;
        lastDisplayedScoreInt = scoreService.Score;
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
        using var bombCreationPositions = matchService.CollectBombCreationPositions();
        using var newlyCreatedBombs = PooledHashSet<SC_Gem>.Get();
        
        matchService.CollectAndDestroyMatchedGems(destroyService);
        bombService.CreateBombs(bombCreationPositions.Value, newlyCreatedBombs, this);
        
        yield return DestroyExplosionsWithDelay(newlyCreatedBombs);
        
        yield return DecreaseRowCo();
    }

    private IEnumerator DestroyExplosionsWithDelay(PooledHashSet<SC_Gem> newlyCreatedBombs)
    {
        using var nonBombExplosions = matchService.CollectNonBombExplosions(newlyCreatedBombs);
        if (nonBombExplosions.Value.Count > 0)
        {
            yield return WaitForSecondsPool.Get(Settings.bombNeighborDelay);
            destroyService.DestroyGems(nonBombExplosions.Value);
        }
        
        using var bombExplosions = matchService.CollectBombExplosions(newlyCreatedBombs);
        if (bombExplosions.Value.Count > 0)
        {
            yield return WaitForSecondsPool.Get(Settings.bombSelfDelay);
            destroyService.DestroyGems(bombExplosions.Value);
            yield return WaitForSecondsPool.Get(Settings.bombPostSelfDelay);
        }
    }
    private IEnumerator DecreaseRowCo()
    {
        yield return WaitForSecondsPool.Get(Settings.decreaseRowDelay);

        bool hasActivity = true;
        while (hasActivity)
        {
            boardService.SpawnTopRow(this);
            hasActivity = boardService.DropSingleRow(this);

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

    private void CheckMisplacedGems()
    {
        using var foundGems = PooledHashSet<SC_Gem>.Get();
        foundGems.Value.UnionWith(FindObjectsOfType<SC_Gem>());
        
        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                SC_Gem _curGem = gameBoard.GetGem(x, y);
                if (_curGem != null)
                {
                    foundGems.Value.Remove(_curGem);
                }
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
