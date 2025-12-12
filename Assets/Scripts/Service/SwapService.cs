using Cysharp.Threading.Tasks;
using Domain.Interfaces;
using Service.Interfaces;

namespace Service
{
    public class SwapService : ISwapService
    {
        private const float POSITION_THRESHOLD = 0.01f;
        
        private readonly IGameBoard gameBoard;
        private readonly IGameStateProvider gameStateProvider;
        private readonly IMatchService matchService;
        private readonly IMatchDispatcher matchDispatcher;
        private readonly ISettings settings;
        
        private IPiece gem;
        private IPiece otherGem;
        
        public SwapService(IGameBoard gameBoard,
            IGameStateProvider gameStateProvider,
            IMatchService matchService,
            IMatchDispatcher matchDispatcher,
            ISettings settings)
        {
            this.gameBoard = gameBoard;
            this.gameStateProvider = gameStateProvider;
            this.matchService = matchService;
            this.matchDispatcher = matchDispatcher;
            this.settings = settings;
        }
        
        public void MovePieces(IPiece gem)
        {
            this.gem = gem;
            
            GetOtherGem(gem as IGemView);

            gameBoard.SetGem(gem.Position, gem);
            gameBoard.SetGem(otherGem.Position, otherGem);

            CheckMoveAsync(gem as IGemView, otherGem as IGemView).Forget();
        }
        
        private void GetOtherGem(IGemView gemView)
        {
            switch (gemView.SwapAngle)
            {
                case < 45 and > -45 when gem.Position.X < settings.RowsSize - 1:
                {
                    otherGem = gameBoard.GetGem(gem.Position.X + 1, gem.Position.Y);
                    otherGem.IsSwapMovement = true;
                    otherGem.Position.X--;
                    gem.PrevPosition = gem.Position;
                    gem.Position.X++;
                    break;
                }
                case > 45 and <= 135 when gem.Position.Y < settings.ColsSize - 1:
                {
                    otherGem = gameBoard.GetGem(gem.Position.X, gem.Position.Y + 1);
                    otherGem.IsSwapMovement = true;
                    otherGem.Position.Y--;
                    gem.PrevPosition = gem.Position;
                    gem.Position.Y++;
                    break;
                }
                case < -45 and >= -135 when gem.Position.Y > 0:
                {
                    otherGem = gameBoard.GetGem(gem.Position.X, gem.Position.Y - 1);
                    otherGem.IsSwapMovement = true;
                    otherGem.Position.Y++;
                    gem.PrevPosition = gem.Position;
                    gem.Position.Y--;
                    break;
                }
                case > 135 or < -135 when gem.Position.X > 0:
                {
                    otherGem = gameBoard.GetGem(gem.Position.X - 1, gem.Position.Y);
                    otherGem.IsSwapMovement = true;
                    otherGem.Position.X++;
                    gem.PrevPosition = gem.Position;
                    gem.Position.X--;
                    break;
                }
            }
        }

        private async UniTask CheckMoveAsync(IGemView gemView, IGemView otherGemView)
        {
            gameStateProvider.SetState(GameState.wait);

            await WaitForSwapCompletion(gemView, otherGemView);
            matchService.FindAllMatches(gem.Position, otherGem.Position);

            if (otherGem != null)
            {
                if (gem.IsMatch == false && otherGem.IsMatch == false)
                {
                    otherGem.Position = gem.Position;
                    gem.Position = gem.PrevPosition;
                    gem.IsSwapMovement = true;
                    otherGem.IsSwapMovement = true;

                    gameBoard.SetGem(gem.Position, gem);
                    gameBoard.SetGem(otherGem.Position, otherGem);

                    await WaitForSwapCompletion(gemView, otherGemView);
                    gameStateProvider.SetState(GameState.move);
                }
                else
                {
                    matchDispatcher.DestroyMatches();
                }
            }
        }

        private async UniTask WaitForSwapCompletion(IGemView gemView, IGemView otherGemView)
        {
            while (gemView.TargetPositionDistance > POSITION_THRESHOLD ||
                   otherGemView?.TargetPositionDistance > POSITION_THRESHOLD)
            {
                await UniTask.Yield();
            }
        }
    }
}

