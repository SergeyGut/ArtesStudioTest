using System.Collections.Generic;

public class GameBoard : IGameBoard
{
    private readonly int height = 0;
    private readonly int width = 0;
    private readonly HashSet<IPiece> explosions = new();
    private readonly List<MatchInfo> matchInfoMap = new();
    private readonly IPiece[,] allGems;
    
    public int Width => width;
    public int Height => height;
    public HashSet<IPiece> Explosions => explosions;
    public List<MatchInfo> MatchInfoMap => matchInfoMap;

    public GameBoard(IBoardSettings settings)
    {
        height = settings.RowsSize;
        width = settings.ColsSize;
        allGems = new IPiece[width, height];
    }
    
    public int GetMatchCountAt(GridPosition _PositionToCheck, IPiece _GemToCheck)
    {
        int horizontalMatches = CountHorizontalMatch(_PositionToCheck, _GemToCheck);
        int verticalMatches = CountVerticalMatch(_PositionToCheck, _GemToCheck);
        return (horizontalMatches >= 2 ? horizontalMatches : 0) + 
               (verticalMatches >= 2 ? verticalMatches : 0);
    }
    
    private int CountHorizontalMatch(GridPosition pos, IPiece gemToCheck)
    {
        int leftCount = CountMatchingGemsInDirection(pos, -1, 0, gemToCheck.Type);
        int rightCount = CountMatchingGemsInDirection(pos, 1, 0, gemToCheck.Type);
        
        return leftCount + rightCount;
    }

    private int CountVerticalMatch(GridPosition pos, IPiece gemToCheck)
    {
        int belowCount = CountMatchingGemsInDirection(pos, 0, -1, gemToCheck.Type);
        int aboveCount = CountMatchingGemsInDirection(pos, 0, 1, gemToCheck.Type);
        
        return belowCount + aboveCount;
    }

    private int CountMatchingGemsInDirection(GridPosition startPos, int deltaX, int deltaY, GemType typeToMatch)
    {
        int count = 0;
        int x = startPos.X + deltaX;
        int y = startPos.Y + deltaY;

        while (IsValidPosition(x, y) && allGems[x, y] != null && allGems[x, y].Type == typeToMatch)
        {
            count++;
            x += deltaX;
            y += deltaY;
        }

        return count;
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public void SetGem(int _X, int _Y, IPiece _Gem)
    {
        allGems[_X, _Y] = _Gem;
    }
    public IPiece GetGem(int _X,int _Y)
    {
       return allGems[_X, _Y];
    }

    public void SetGem(GridPosition position, IPiece gem)
    {
        SetGem(position.X, position.Y, gem);
    }

    public IPiece GetGem(GridPosition position)
    {
        return GetGem(position.X, position.Y);
    }

    public void FindAllMatches(GridPosition? userActionPos = null)
    {
        explosions.Clear();
        foreach (var matchInfo in matchInfoMap)
        {
            if (matchInfo.MatchedGems != null)
                CollectionPool<HashSet<IPiece>>.Release(matchInfo.MatchedGems);
        }
        matchInfoMap.Clear();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                IPiece currentGem = allGems[x, y];
                if (currentGem != null)
                {
                    HashSet<IPiece> horizontalMatches = CheckMatchesInDirection(x, y, 1, 0);
                    HashSet<IPiece> verticalMatches = CheckMatchesInDirection(x, y, 0, 1);

                    if (horizontalMatches != null)
                    {
                        AddMatch(new MatchInfo { MatchedGems = horizontalMatches, UserActionPos = userActionPos });
                    }

                    if (verticalMatches != null)
                    {
                        AddMatch(new MatchInfo { MatchedGems = verticalMatches, UserActionPos = userActionPos });
                    }
                }
            }

        CheckForBombs();
    }

    private void AddMatch(MatchInfo newMatch)
    {
        foreach (var gem in newMatch.MatchedGems)
        {
            MarkGemAsMatched(gem);
        }
        
        for (int i = 0; i < matchInfoMap.Count; ++i)
        {
            if (matchInfoMap[i].MatchedGems.Overlaps(newMatch.MatchedGems))
            {
                matchInfoMap[i].MatchedGems.UnionWith(newMatch.MatchedGems);
                matchInfoMap[i].UserActionPos ??= newMatch.UserActionPos;
                if (newMatch.MatchedGems != matchInfoMap[i].MatchedGems)
                    CollectionPool<HashSet<IPiece>>.Release(newMatch.MatchedGems);
                return;
            }
        }
        
        matchInfoMap.Add(newMatch);
    }

    private HashSet<IPiece> CheckMatchesInDirection(int x, int y, int deltaX, int deltaY)
    {
        IPiece currentGem = allGems[x, y];
        
        if (currentGem == null)
            return null;

        var matches = CollectionPool<HashSet<IPiece>>.Get();
        matches.Add(currentGem);

        foreach (var gem in GetMatchingGemsInDirection(x, y, deltaX, deltaY, currentGem.Type))
        {
            matches.Add(gem);
        }

        foreach (var gem in GetMatchingGemsInDirection(x, y, -deltaX, -deltaY, currentGem.Type))
        {
            matches.Add(gem);
        }

        if (matches.Count < 3)
        {
            CollectionPool<HashSet<IPiece>>.Release(matches);
            return null;
        }

        return matches;
    }

    private IEnumerable<IPiece> GetMatchingGemsInDirection(int startX, int startY, int deltaX, int deltaY, GemType typeToMatch)
    {
        int x = startX + deltaX;
        int y = startY + deltaY;

        while (IsValidPosition(x, y) && allGems[x, y] != null && allGems[x, y].Type == typeToMatch)
        {
            yield return allGems[x, y];
            x += deltaX;
            y += deltaY;
        }
    }

    private void CheckForBombs()
    {
        foreach (var matchInfo in MatchInfoMap)
        foreach (var gem in matchInfo.MatchedGems)
        {
            int x = gem.Position.X;
            int y = gem.Position.Y;

            if (gem.Position.X > 0)
            {
                if (allGems[x - 1, y] != null && allGems[x - 1, y].Type == GemType.bomb)
                    MarkBombArea(new GridPosition(x - 1, y), allGems[x - 1, y].BlastSize);
            }

            if (gem.Position.X + 1 < width)
            {
                if (allGems[x + 1, y] != null && allGems[x + 1, y].Type == GemType.bomb)
                    MarkBombArea(new GridPosition(x + 1, y), allGems[x + 1, y].BlastSize);
            }

            if (gem.Position.Y > 0)
            {
                if (allGems[x, y - 1] != null && allGems[x, y - 1].Type == GemType.bomb)
                    MarkBombArea(new GridPosition(x, y - 1), allGems[x, y - 1].BlastSize);
            }

            if (gem.Position.Y + 1 < height)
            {
                if (allGems[x, y + 1] != null && allGems[x, y + 1].Type == GemType.bomb)
                    MarkBombArea(new GridPosition(x, y + 1), allGems[x, y + 1].BlastSize);
            }
        }
    }

    private void MarkBombArea(GridPosition bombPos, int _BlastSize)
    {
        for (int x = bombPos.X - _BlastSize; x <= bombPos.X + _BlastSize; x++)
        {
            for (int y = bombPos.Y - _BlastSize; y <= bombPos.Y + _BlastSize; y++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    var gem = allGems[x, y];
                    
                    if (gem != null)
                    {
                        MarkGemAsMatched(gem);
                        explosions.Add(gem);
                    }
                }
            }
        }
    }

    private void MarkColorBombArea(GridPosition bombPos, int _BlastSize)
    {
        int sqrBlastSize = _BlastSize * _BlastSize;
        for (int x = bombPos.X - _BlastSize; x <= bombPos.X + _BlastSize; x++)
        {
            for (int y = bombPos.Y - _BlastSize; y <= bombPos.Y + _BlastSize; y++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    var gem = allGems[x, y];

                    if (gem != null)
                    {
                        int dx = x - bombPos.X;
                        int dy = y - bombPos.Y;
                        int sqrDistance = dx * dx + dy * dy;
                        if (sqrDistance > sqrBlastSize)
                        {
                            continue;
                        }

                        MarkGemAsMatched(gem);
                        explosions.Add(gem);
                    }
                }
            }
        }
    }
    
    private void MarkGemAsMatched(IPiece gem)
    {
        if (gem is { IsMatch: false })
        {
            gem.IsMatch = true;
            
            if (gem.IsColorBomb)
            {
                MarkColorBombArea(gem.Position, gem.BlastSize);
                return;
            }

            if (gem.Type == GemType.bomb)
            {
                MarkBombArea(gem.Position, gem.BlastSize);
            }
        }
    }
}

