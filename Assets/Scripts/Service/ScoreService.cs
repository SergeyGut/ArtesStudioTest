using Service.Interfaces;

namespace Service
{
    public class ScoreService : IScoreService
    {
        private int score;

        public int Score => score;

        public void AddScore(int points)
        {
            score += points;
        }
    }
}
