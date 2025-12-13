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
                IPiece gem = gameBoard.GetGem(x, y);

                if (gem == null)
                {
                    continue;
                }
                
                if (gem is { IsMoving: true })
                {
                    anyDropped = true;
                    continue;
                }

                RunDropAsync(gem).Forget();

                if (gem.IsMoving)
                {
                    anyDropped = true;
                    break;
                }
            }

            return anyDropped;
        }
        
        public async UniTask RunDropAsync(IPiece gem)
        {
            while (gem.Position.Y > 0)
            {
                int x = gem.Position.X;
                int y = gem.Position.Y;

                IPiece gemBelow = gameBoard.GetGem(x, y - 1);

                if (gemBelow == null)
                {
                    gem.IsMoving = true;
                    gem.Position.Y--;
                    
                    gameBoard.SetGem(x, y - 1, gem);
                    if (gameBoard.IsValidPosition(x, y))
                    {
                        gameBoard.SetGem(x, y, null);
                    }
                    
                    await UniTask.
                        WaitForSeconds(1f / settings.GemSpeed, cancellationToken: gem.Token).
                        SuppressCancellationThrow();
                    
                    if (gem.Token.IsCancellationRequested)
                    {
                        return;
                    }
                }
                else
                {
                    gem.IsMoving = false;
                    return;
                }
            }
            
            gem.IsMoving = false;
        }
    }
}
