using Cysharp.Threading.Tasks;
using Domain.Interfaces;
using Service.Interfaces;

namespace Service
{
    public class DropService : IDropService
    {
        private readonly IGameBoard gameBoard;
        private readonly ISettings settings;

        public DropService(
            IGameBoard gameBoard,
            ISettings settings)
        {
            this.gameBoard = gameBoard;
            this.settings = settings;
        }

        public bool DropSingleX(int x)
        {
            bool anyDropped = false;

            for (int y = 1; y < gameBoard.Height; y++)
            {
                IPiece piece = gameBoard.GetPiece(x, y);

                if (piece == null)
                {
                    continue;
                }
                
                if (piece is { IsMoving: true })
                {
                    anyDropped = true;
                    continue;
                }

                RunDropAsync(piece).Forget();

                if (piece.IsMoving)
                {
                    anyDropped = true;
                    break;
                }
            }

            return anyDropped;
        }
        
        public async UniTask RunDropAsync(IPiece piece)
        {
            while (piece.Position.Y > 0)
            {
                int x = piece.Position.X;
                int y = piece.Position.Y;

                IPiece pieceBelow = gameBoard.GetPiece(x, y - 1);

                if (pieceBelow == null)
                {
                    piece.IsMoving = true;
                    piece.Position.Y--;
                    
                    gameBoard.SetPiece(x, y - 1, piece);
                    if (gameBoard.IsValidPosition(x, y))
                    {
                        gameBoard.SetPiece(x, y, null);
                    }
                    
                    await UniTask.
                        WaitForSeconds(1f / settings.PieceSpeed, cancellationToken: piece.Token).
                        SuppressCancellationThrow();
                    
                    if (piece.Token.IsCancellationRequested)
                    {
                        return;
                    }
                }
                else
                {
                    piece.IsMoving = false;
                    return;
                }
            }
            
            piece.IsMoving = false;
        }
    }
}
