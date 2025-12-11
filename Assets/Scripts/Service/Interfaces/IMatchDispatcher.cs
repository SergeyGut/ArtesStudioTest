
public interface IMatchDispatcher
{
    void FindAllMatches(GridPosition? posIndex = null, GridPosition? otherPosIndex = null);
    void DestroyMatches();
}

