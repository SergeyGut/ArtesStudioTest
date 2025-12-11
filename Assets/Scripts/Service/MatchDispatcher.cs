using Cysharp.Threading.Tasks;
using Zenject;

public class MatchDispatcher : IMatchDispatcher
{
    private IGameBoard gameBoard;
    private IPathfinderService pathfinderService;
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
        IPathfinderService _PathfinderService,
        ISpawnService spawnService,
        IDestroyService destroyService,
        IBombService bombService,
        IDropService dropService,
        ISettings settings,
        IBoardView boardView,
        IGameStateProvider gameStateProvider)
    {
        this.gameBoard = gameBoard;
        this.pathfinderService = _PathfinderService;
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
        DestroyMatchesAsync().Forget();
    }

    private async UniTask DestroyMatchesAsync()
    {
        using var bombCreationPositions = pathfinderService.CollectBombCreationPositions();
        using var newlyCreatedBombs = PooledHashSet<IPiece>.Get();
        
        using var matchedGems = pathfinderService.CollectMatchedGems();
        destroyService.DestroyGems(matchedGems.Value);
        bombService.CreateBombs(bombCreationPositions.Value, newlyCreatedBombs);
        
        await DestroyExplosionsWithDelayAsync(newlyCreatedBombs);
        await DecreaseRowAsync();
    }

    private async UniTask DestroyExplosionsWithDelayAsync(PooledHashSet<IPiece> newlyCreatedBombs)
    {
        using var nonBombExplosions = pathfinderService.CollectNonBombExplosions(newlyCreatedBombs);
        if (nonBombExplosions.Value.Count > 0)
        {
            await UniTask.WaitForSeconds(settings.BombNeighborDelay);
            destroyService.DestroyGems(nonBombExplosions.Value);
        }
        
        using var bombExplosions = pathfinderService.CollectBombExplosions(newlyCreatedBombs);
        if (bombExplosions.Value.Count > 0)
        {
            await UniTask.WaitForSeconds(settings.BombSelfDelay);
            destroyService.DestroyGems(bombExplosions.Value);
            await UniTask.WaitForSeconds(settings.BombPostSelfDelay);
        }
    }
    
    private async UniTask DecreaseRowAsync()
    {
        await UniTask.WaitForSeconds(settings.DecreaseRowDelay);

        bool useColumnDelay = settings.DecreaseSingleColumnDelay > 0f;
        using var decreaseTasks = PooledList<UniTask>.Get();
        for (int x = 0; x < gameBoard.Width; x++)
        {
            var task = DecreaseColumnAsync(x);
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

    private async UniTask DecreaseColumnAsync(int  x)
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
