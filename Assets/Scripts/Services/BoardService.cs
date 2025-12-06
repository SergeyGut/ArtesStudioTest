public class BoardService
{
    private readonly IGameBoard gameBoard;
    private readonly SpawnService spawnService;
    
    public BoardService(IGameBoard gameBoard, SpawnService spawnService)
    {
        this.gameBoard = gameBoard;
        this.spawnService = spawnService;
    }
    
    public bool DropSingleRow(IGameLogic gameLogic)
    {
        bool anyDropped = false;

        for (int y = 1; y < gameBoard.Height; y++)
        {
            for (int x = 0; x < gameBoard.Width; x++)
            {
                SC_Gem currentGem = gameBoard.GetGem(x, y);
                SC_Gem gemBelow = gameBoard.GetGem(x, y - 1);

                if (currentGem != null && gemBelow == null)
                {
                    currentGem.posIndex.y--;
                    gameBoard.SetGem(x, y - 1, currentGem);
                    gameBoard.SetGem(x, y, null);
                    anyDropped = true;
                }
            }

            if (anyDropped)
            {
                return true;
            }
        }

        return false;
    }
    
    public void SpawnTopRow(IGameLogic gameLogic)
    {
        spawnService.SpawnTopRow(gameLogic);
    }
}

