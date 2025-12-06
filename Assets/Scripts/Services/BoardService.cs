public class BoardService
{
    private readonly IGameBoard gameBoard;
    private readonly ISpawnService spawnService;
    
    public BoardService(IGameBoard gameBoard, ISpawnService spawnService)
    {
        this.gameBoard = gameBoard;
        this.spawnService = spawnService;
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
    
    public void SpawnTopRow(IGameLogic gameLogic)
    {
        spawnService.SpawnTopRow(gameLogic);
    }
}

