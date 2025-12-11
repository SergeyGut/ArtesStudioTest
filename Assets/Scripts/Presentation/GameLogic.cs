using Cysharp.Threading.Tasks;
using Zenject;

public class GameLogic : IGameLogic
{
    private IGameBoard gameBoard;
    private IMatchService matchService;
    private ISpawnService spawnService;
    private IDestroyService destroyService;
    private IBombService bombService;
    private IDropService dropService;
    private ISettings settings;
    private IBoardView boardView;
    private IGameStateProvider gameStateProvider;
    
    [Inject]
    private void Construct(
        IGameBoard gameBoard,
        IMatchService matchService,
        ISpawnService spawnService,
        IDestroyService destroyService,
        IBombService bombService,
        IDropService dropService,
        ISettings settings,
        IBoardView boardView,
        IGameStateProvider gameStateProvider)
    {
        this.gameBoard = gameBoard;
        this.matchService = matchService;
        this.spawnService = spawnService;
        this.destroyService = destroyService;
        this.bombService = bombService;
        this.dropService = dropService;
        this.settings = settings;
        this.boardView = boardView;
        this.gameStateProvider = gameStateProvider;
    }

    public void DestroyMatches()
    {
        DestroyMatchesCo().Forget();
    }

    private async UniTask DestroyMatchesCo()
    {
        using var bombCreationPositions = matchService.CollectBombCreationPositions();
        using var newlyCreatedBombs = PooledHashSet<IPiece>.Get();
        
        matchService.CollectAndDestroyMatchedGems(destroyService);
        bombService.CreateBombs(bombCreationPositions.Value, newlyCreatedBombs);
        
        await DestroyExplosionsWithDelay(newlyCreatedBombs);
        
        await DecreaseRowCo();
    }

    private async UniTask DestroyExplosionsWithDelay(PooledHashSet<IPiece> newlyCreatedBombs)
    {
        using var nonBombExplosions = matchService.CollectNonBombExplosions(newlyCreatedBombs);
        if (nonBombExplosions.Value.Count > 0)
        {
            await UniTask.WaitForSeconds(settings.BombNeighborDelay);
            destroyService.DestroyGems(nonBombExplosions.Value);
        }
        
        using var bombExplosions = matchService.CollectBombExplosions(newlyCreatedBombs);
        if (bombExplosions.Value.Count > 0)
        {
            await UniTask.WaitForSeconds(settings.BombSelfDelay);
            destroyService.DestroyGems(bombExplosions.Value);
            await UniTask.WaitForSeconds(settings.BombPostSelfDelay);
        }
    }
    private async UniTask DecreaseRowCo()
    {
        await UniTask.WaitForSeconds(settings.DecreaseRowDelay);

        bool useColumnDelay = settings.DecreaseSingleColumnDelay > 0f;
        using var decreaseTasks = PooledList<UniTask>.Get();
        for (int x = 0; x < gameBoard.Width; x++)
        {
            var task = DecreaseColumn(x);
            decreaseTasks.Value.Add(task);

            if (useColumnDelay && !task.GetAwaiter().IsCompleted)
            {
                await UniTask.WaitForSeconds(settings.DecreaseSingleColumnDelay);
            }
        }

        await UniTask.WhenAll(decreaseTasks.Value);

        boardView.CheckMisplacedGems();
        
        await UniTask.WaitForSeconds(settings.FindAllMatchesDelay);
        
        gameBoard.FindAllMatches();
        if (gameBoard.MatchInfoMap.Count > 0)
        {
            await UniTask.WaitForSeconds(settings.DestroyMatchesDelay);
            DestroyMatches();
        }
        else
        {
            await UniTask.WaitForSeconds(settings.ChangeStateDelay);
            gameStateProvider.SetState(GameState.move);
        }
    }

    private async UniTask DecreaseColumn(int  x)
    {
        bool hasActivity = true;
        while (hasActivity)
        {
            spawnService.SpawnTopX(x);
            hasActivity = dropService.DropSingleX(x);

            if (hasActivity)
            {
                await UniTask.WaitForSeconds(settings.DecreaseSingleRowDelay);
            }
        }
    }
    
    public void FindAllMatches(GridPosition? posIndex = null, GridPosition? otherPosIndex = null)
    {
        if (posIndex.HasValue)
            gameBoard.FindAllMatches(posIndex);
        else if (otherPosIndex.HasValue)
            gameBoard.FindAllMatches(otherPosIndex);
        else
            gameBoard.FindAllMatches();
    }
}
