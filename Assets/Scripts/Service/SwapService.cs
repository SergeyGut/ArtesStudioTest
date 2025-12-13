using Cysharp.Threading.Tasks;
using Domain.Interfaces;
using Service.Interfaces;

namespace Service
{
    public class SwapService : ISwapService
    {
        private readonly IGameBoard gameBoard;
        private readonly IBoardView boardView;
        private readonly IGameStateProvider gameStateProvider;
        private readonly IMatchService matchService;
        private readonly IMatchDispatcher matchDispatcher;
        private readonly ISettings settings;
        
        private IPiece gem;
        private IPiece otherGem;
        
        public SwapService(
            IGameBoard gameBoard,
            IBoardView boardView,
            IGameStateProvider gameStateProvider,
            IMatchService matchService,
            IMatchDispatcher matchDispatcher,
            ISettings settings)
        {
            this.gameBoard = gameBoard;
            this.boardView = boardView;
            this.gameStateProvider = gameStateProvider;
            this.matchService = matchService;
            this.matchDispatcher = matchDispatcher;
            this.settings = settings;
        }
        
        public void MovePieces(IPiece piece)
        {
            gem = piece;

            var gemView = boardView.GetPieceView(gem);
            
            GetOtherGem(gemView);

            gameBoard.SetGem(gem.Position, gem);
            gameBoard.SetGem(otherGem.Position, otherGem);

            var otherGemView = boardView.GetPieceView(otherGem);
            
            CheckMoveAsync(gemView, otherGemView).Forget();
        }
        
        private void GetOtherGem(IPieceView gemView)
        {
            switch (gemView.SwapAngle)
            {
                case < 45 and > -45 when gem.Position.X < settings.RowsSize - 1:
                {
                    otherGem = gameBoard.GetGem(gem.Position.X + 1, gem.Position.Y);
                    otherGem.Position.X--;
                    gem.PrevPosition = gem.Position;
                    gem.Position.X++;
                    break;
                }
                case > 45 and <= 135 when gem.Position.Y < settings.ColsSize - 1:
                {
                    otherGem = gameBoard.GetGem(gem.Position.X, gem.Position.Y + 1);
                    otherGem.Position.Y--;
                    gem.PrevPosition = gem.Position;
                    gem.Position.Y++;
                    break;
                }
                case < -45 and >= -135 when gem.Position.Y > 0:
                {
                    otherGem = gameBoard.GetGem(gem.Position.X, gem.Position.Y - 1);
                    otherGem.Position.Y++;
                    gem.PrevPosition = gem.Position;
                    gem.Position.Y--;
                    break;
                }
                case > 135 or < -135 when gem.Position.X > 0:
                {
                    otherGem = gameBoard.GetGem(gem.Position.X - 1, gem.Position.Y);
                    otherGem.Position.X++;
                    gem.PrevPosition = gem.Position;
                    gem.Position.X--;
                    break;
                }
                default: return;
            }
            
            gem.IsSwap = true;
            otherGem.IsSwap = true;
        }

        private async UniTask CheckMoveAsync(IPieceView gemView, IPieceView otherGemView)
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
                    
                    gem.IsSwap = true;
                    otherGem.IsSwap = true;

                    gameBoard.SetGem(gem.Position, gem);
                    gameBoard.SetGem(otherGem.Position, otherGem);

                    await WaitForSwapCompletion(gemView, otherGemView);
                    
                    gem.IsSwap = false;
                    otherGem.IsSwap = false;

                    gameStateProvider.SetState(GameState.move);
                }
                else
                {
                    matchDispatcher.DestroyMatches();
                }
            }
        }

        private async UniTask WaitForSwapCompletion(IPieceView gemView, IPieceView otherGemView)
        {
            while (!gemView.TargetPositionArrived || !otherGemView.TargetPositionArrived)
            {
                await UniTask.Yield();
            }
        }
    }
}

