
using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface IGameStateProvider
    {
        GameState CurrentState { get; }
        void SetState(GameState state);
    }
}