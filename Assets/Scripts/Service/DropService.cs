
public class DropService : IDropService
{
    private readonly IGameBoard gameBoard;
    
    public DropService(IGameBoard gameBoard)
    {
        this.gameBoard = gameBoard;
    }
    
    public bool DropSingleX(int x)
    {
        bool anyDropped = false;
        
        for (int y = 1; y < gameBoard.Height; y++)
        {
            IPiece currentGem = gameBoard.GetGem(x, y);
            IPiece gemBelow = gameBoard.GetGem(x, y - 1);

            if (currentGem != null && (currentGem.IsMoving || currentGem.JustSpawned))
            {
                anyDropped = true;
                continue;
            }
            
            if (currentGem != null && gemBelow == null)
            {
                currentGem.Position.Y--;
                gameBoard.SetGem(x, y - 1, currentGem);
                gameBoard.SetGem(x, y, null);
                anyDropped = true;
                break;
            }
        }

        return anyDropped;
    }
}

