

using System.Collections.Generic;

public interface IMatchService
{
    HashSet<IPiece> Explosions { get; }
    List<MatchInfo> MatchInfoMap { get; }
    int GetMatchCountAt(GridPosition _PositionToCheck, IPiece _GemToCheck);
    void FindAllMatches(GridPosition? userActionPos = null);
    void FindAllMatches(GridPosition? posIndex, GridPosition? otherPosIndex);

}

