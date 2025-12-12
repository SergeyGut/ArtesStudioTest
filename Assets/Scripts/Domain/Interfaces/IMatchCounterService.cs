
namespace Domain.Interfaces
{
    public interface IMatchCounterService
    {
        int GetMatchCountAt(GridPosition positionToCheck, IPiece gemToCheck);
    }    
}

