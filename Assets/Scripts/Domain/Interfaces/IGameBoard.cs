using System.Collections.Generic;

public interface IGameBoard
{
    int Width { get; }
    int Height { get; }
    HashSet<IPiece> Explosions { get; }
    List<MatchInfo> MatchInfoMap { get; }

    int GetMatchCountAt(GridPosition _PositionToCheck, IPiece _GemToCheck);
    void SetGem(int x, int y, IPiece gem);
    IPiece GetGem(int x, int y);
    void SetGem(GridPosition position, IPiece gem);
    IPiece GetGem(GridPosition position);
    void FindAllMatches(GridPosition? userActionPos = null);
}


