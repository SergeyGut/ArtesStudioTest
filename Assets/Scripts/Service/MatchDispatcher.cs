using Cysharp.Threading.Tasks;
using Domain.Interfaces;
using Domain.Pool;
using Service.Interfaces;
using Zenject;

namespace Service
{
    public class MatchDispatcher : IMatchDispatcher
    {
        private readonly IGameBoard gameBoard;
        private readonly IMatchService matchService;
        private readonly IPathfinderService pathfinderService;
        private readonly ISpawnService spawnService;
        private readonly IDestroyService destroyService;
        private readonly IBombService bombService;
        private readonly IDropService dropService;
        private readonly ISettings settings;
        private readonly IBoardView boardView;
        private readonly IGameStateProvider gameStateProvider;

        public MatchDispatcher(
            IGameBoard gameBoard,
            IMatchService matchService,
            IPathfinderService pathfinderService,
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
            this.pathfinderService = pathfinderService;
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

            matchService.FindAllMatches();
            if (matchService.MatchInfoMap.Count > 0)
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

        private async UniTask DecreaseColumnAsync(int x)
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
    }
}