using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface IReadOnlyGameStateProvider
    {
        GameState CurrentState { get; }
    }
    
    public interface IGameStateProvider : IReadOnlyGameStateProvider
    {
        void SetState(GameState state);
    }
}