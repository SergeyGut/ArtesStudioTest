using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

public class SC_GameLogic : MonoBehaviour, IGameLogic
{
    private Dictionary<string, GameObject> unityObjects;
    private float displayScore = 0;
    private GlobalEnums.GameState currentState = GlobalEnums.GameState.move;
    private TextMeshProUGUI scoreText;
    private float scoreSpeed;
    private int lastDisplayedScoreInt = -1;
    
    private IGameBoard gameBoard;
    private IGemPool gemPool;
    private IMatchService matchService;
    private ISpawnService spawnService;
    private IDestroyService destroyService;
    private IScoreService scoreService;
    private IBombService bombService;
    private IBoardService boardService;

    public GlobalEnums.GameState CurrentState => currentState;

    [Inject]
    private void Construct(
        IGameBoard gameBoard,
        IGemPool gemPool,
        IMatchService matchService,
        ISpawnService spawnService,
        IDestroyService destroyService,
        IScoreService scoreService,
        IBombService bombService,
        IBoardService boardService,
        Dictionary<string, GameObject> unityObjects)
    {
        this.gameBoard = gameBoard;
        this.gemPool = gemPool;
        this.matchService = matchService;
        this.spawnService = spawnService;
        this.destroyService = destroyService;
        this.scoreService = scoreService;
        this.bombService = bombService;
        this.boardService = boardService;
        this.unityObjects = unityObjects;
    }
    
    #region MonoBehaviour
    private void Awake()
    {
        Setup();
    }

    private void Start()
    {
        scoreText = unityObjects["Txt_Score"].GetComponent<TextMeshProUGUI>();
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
                spawnService.SpawnGem(new Vector2Int(x, y), gemToSpawn, this, gameBoard);
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
    public void SetState(GlobalEnums.GameState _CurrentState)
    {
        currentState = _CurrentState;
    }
    public void DestroyMatches()
    {
        DestroyMatchesCo().Forget();
    }

    private async UniTask DestroyMatchesCo()
    {
        using var bombCreationPositions = matchService.CollectBombCreationPositions();
        using var newlyCreatedBombs = PooledHashSet<SC_Gem>.Get();
        
        matchService.CollectAndDestroyMatchedGems(destroyService);
        bombService.CreateBombs(bombCreationPositions.Value, newlyCreatedBombs);
        
        await DestroyExplosionsWithDelay(newlyCreatedBombs);
        
        await DecreaseRowCo();
    }

    private async UniTask DestroyExplosionsWithDelay(PooledHashSet<SC_Gem> newlyCreatedBombs)
    {
        using var nonBombExplosions = matchService.CollectNonBombExplosions(newlyCreatedBombs);
        if (nonBombExplosions.Value.Count > 0)
        {
            await UniTask.WaitForSeconds(Settings.bombNeighborDelay);
            destroyService.DestroyGems(nonBombExplosions.Value);
        }
        
        using var bombExplosions = matchService.CollectBombExplosions(newlyCreatedBombs);
        if (bombExplosions.Value.Count > 0)
        {
            await UniTask.WaitForSeconds(Settings.bombSelfDelay);
            destroyService.DestroyGems(bombExplosions.Value);
            await UniTask.WaitForSeconds(Settings.bombPostSelfDelay);
        }
    }
    private async UniTask DecreaseRowCo()
    {
        await UniTask.WaitForSeconds(Settings.decreaseRowDelay);

        bool useColumnDelay = Settings.decreaseSingleColumnDelay > 0f;
        using var decreaseTasks = PooledList<UniTask>.Get();
        for (int x = 0; x < gameBoard.Width; x++)
        {
            var task = DecreaseColumn(x);
            decreaseTasks.Value.Add(task);

            if (useColumnDelay && !task.GetAwaiter().IsCompleted)
            {
                await UniTask.WaitForSeconds(Settings.decreaseSingleColumnDelay);
            }
        }

        await UniTask.WhenAll(decreaseTasks.Value);

        CheckMisplacedGems();
        await UniTask.WaitForSeconds(Settings.findAllMatchesDelay);
        gameBoard.FindAllMatches();
        if (gameBoard.MatchInfoMap.Count > 0)
        {
            await UniTask.WaitForSeconds(Settings.destroyMatchesDelay);
            DestroyMatches();
        }
        else
        {
            await UniTask.WaitForSeconds(Settings.changeStateDelay);
            currentState = GlobalEnums.GameState.move;
        }
    }

    private async UniTask DecreaseColumn(int  x)
    {
        bool hasActivity = true;
        while (hasActivity)
        {
            spawnService.SpawnTopX(x, this, gameBoard);
            hasActivity = boardService.DropSingleX(x);

            if (hasActivity)
            {
                await UniTask.WaitForSeconds(Settings.decreaseSingleRowDelay);
            }
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
