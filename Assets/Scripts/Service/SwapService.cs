using System.Threading;
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
        
        private IPiece piece;
        private IPiece otherPiece;
        
        private bool IsAnyCancellationRequested => piece.Token.IsCancellationRequested || otherPiece.Token.IsCancellationRequested;
        
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
        
        public void MovePieces(IPieceView pieceView)
        {
            piece = pieceView.Piece as IPiece;
            
            GetOtherGem(pieceView);

            gameBoard.SetGem(piece.Position, piece);
            gameBoard.SetGem(otherPiece.Position, otherPiece);

            var otherGemView = boardView.GetPieceView(otherPiece);
            
            CheckMoveAsync(pieceView, otherGemView).Forget();
        }
        
        private void GetOtherGem(IPieceView gemView)
        {
            switch (gemView.SwapAngle)
            {
                case < 45 and > -45 when piece.Position.X < settings.RowsSize - 1:
                {
                    otherPiece = gameBoard.GetGem(piece.Position.X + 1, piece.Position.Y);
                    otherPiece.Position.X--;
                    piece.PrevPosition = piece.Position;
                    piece.Position.X++;
                    break;
                }
                case > 45 and <= 135 when piece.Position.Y < settings.ColsSize - 1:
                {
                    otherPiece = gameBoard.GetGem(piece.Position.X, piece.Position.Y + 1);
                    otherPiece.Position.Y--;
                    piece.PrevPosition = piece.Position;
                    piece.Position.Y++;
                    break;
                }
                case < -45 and >= -135 when piece.Position.Y > 0:
                {
                    otherPiece = gameBoard.GetGem(piece.Position.X, piece.Position.Y - 1);
                    otherPiece.Position.Y++;
                    piece.PrevPosition = piece.Position;
                    piece.Position.Y--;
                    break;
                }
                case > 135 or < -135 when piece.Position.X > 0:
                {
                    otherPiece = gameBoard.GetGem(piece.Position.X - 1, piece.Position.Y);
                    otherPiece.Position.X++;
                    piece.PrevPosition = piece.Position;
                    piece.Position.X--;
                    break;
                }
                default: return;
            }
            
            piece.IsSwap = true;
            otherPiece.IsSwap = true;
        }

        private async UniTask CheckMoveAsync(IPieceView pieceView, IPieceView otherPieceView)
        {
            gameStateProvider.SetState(GameState.wait);

            await WaitForSwapCompletion(pieceView, otherPieceView);
            
            if (IsAnyCancellationRequested)
            {
                return;
            }
            
            matchService.FindAllMatches(piece.Position, otherPiece.Position);

            if (otherPiece != null)
            {
                if (piece.IsMatch == false && otherPiece.IsMatch == false)
                {
                    otherPiece.Position = piece.Position;
                    piece.Position = piece.PrevPosition;
                    
                    piece.IsSwap = true;
                    otherPiece.IsSwap = true;

                    gameBoard.SetGem(piece.Position, piece);
                    gameBoard.SetGem(otherPiece.Position, otherPiece);

                    await WaitForSwapCompletion(pieceView, otherPieceView);
                    
                    if (IsAnyCancellationRequested)
                    {
                        return;
                    }
                    
                    piece.IsSwap = false;
                    otherPiece.IsSwap = false;

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
                if (IsAnyCancellationRequested)
                {
                    return;
                }
                
                await UniTask.Yield();
            }
        }
    }
}

