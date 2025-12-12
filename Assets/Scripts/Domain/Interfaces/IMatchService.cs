

using System.Collections.Generic;

public interface IMatchService
{
    HashSet<IPiece> Explosions { get; }
    List<MatchInfo> MatchInfoMap { get; }
    void FindAllMatches(GridPosition? userActionPos = null, GridPosition? otherUserActionPos = null);
}
