public interface IScoreService
{
    void AddScore(int points);
    int Score { get; }
}

public class ScoreService : IScoreService
{
    private readonly IGameBoard gameBoard;
    
    public ScoreService(IGameBoard gameBoard)
    {
        this.gameBoard = gameBoard;
    }
    
    public void AddScore(int points)
    {
        gameBoard.Score += points;
    }
    
    public int Score => gameBoard.Score;
}

