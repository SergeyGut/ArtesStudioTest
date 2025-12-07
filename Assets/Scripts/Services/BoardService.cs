
public class BoardService
{
    private readonly IGameBoard gameBoard;
    
    public BoardService(IGameBoard gameBoard)
    {
        this.gameBoard = gameBoard;
    }
    
    public bool DropSingleRow()
    {
        bool anyDropped = false;

        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 1; y < gameBoard.Height; y++)
            {
                SC_Gem currentGem = gameBoard.GetGem(x, y);
                SC_Gem gemBelow = gameBoard.GetGem(x, y - 1);

                if (currentGem != null && (currentGem.isMoving || currentGem.justSpawned))
                {
                    anyDropped = true;
                    continue;
                }
                
                if (currentGem != null && gemBelow == null)
                {
                    currentGem.posIndex.y--;
                    gameBoard.SetGem(x, y - 1, currentGem);
                    gameBoard.SetGem(x, y, null);
                    anyDropped = true;
                    break;
                }
            }
        }

        return anyDropped;
    }
}

