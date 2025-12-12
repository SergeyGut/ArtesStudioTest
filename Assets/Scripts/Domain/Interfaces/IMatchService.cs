using System.Collections.Generic;

namespace Domain.Interfaces
{
    public interface IMatchService
    {
        HashSet<IPiece> Explosions { get; }
        List<MatchInfo> MatchInfoMap { get; }
        void FindAllMatches(GridPosition? userActionPos = null, GridPosition? otherUserActionPos = null);
    }
}
