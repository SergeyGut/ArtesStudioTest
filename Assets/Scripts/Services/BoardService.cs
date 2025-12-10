
public class BoardService : IBoardService
{
    private readonly IGameBoard gameBoard;
    
    public BoardService(IGameBoard gameBoard)
    {
        this.gameBoard = gameBoard;
    }
    
    public bool DropSingleX(int x)
    {
        bool anyDropped = false;
        
        for (int y = 1; y < gameBoard.Height; y++)
        {
            SC_Gem currentGem = gameBoard.GetGem(x, y);
            SC_Gem gemBelow = gameBoard.GetGem(x, y - 1);

            if (currentGem != null && (currentGem.IsMoving || currentGem.JustSpawned))
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

        return anyDropped;
    }
}

