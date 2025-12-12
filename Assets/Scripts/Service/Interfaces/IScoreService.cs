
namespace Service.Interfaces
{
    public interface IScoreService
    {
        void AddScore(int points);
        int Score { get; }
    }
}