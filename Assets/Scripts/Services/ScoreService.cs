public interface IScoreService
{
    void AddScore(int points);
    int Score { get; }
}

public class ScoreService : IScoreService
{
    private int score = 0;
    
    public void AddScore(int points)
    {
        score += points;
    }
    
    public int Score => score;
}

