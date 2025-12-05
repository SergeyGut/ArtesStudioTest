using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GameBoard
{
    #region Variables

    private readonly int height = 0;
    public int Height => height;

    private readonly int width = 0;
    public int Width => width;

    private SC_Gem[,] allGems;

    private int score = 0;
    public int Score 
    {
        get => score;
        set => score = value;
    }

    private readonly HashSet<SC_Gem> explosions = new();
    public HashSet<SC_Gem> Explosions => explosions;

    private readonly List<MatchInfo> matchInfoMap = new();
    public List<MatchInfo> MatchInfoMap => matchInfoMap;

    public class MatchInfo
    {
        public HashSet<SC_Gem> MatchedGems;
        public Vector2Int? UserActionPos;
    }
    #endregion

    public GameBoard(int _Width, int _Height)
    {
        height = _Height;
        width = _Width;
        allGems = new SC_Gem[width, height];
    }
    public bool MatchesAt(Vector2Int _PositionToCheck, SC_Gem _GemToCheck)
    {
        return CheckHorizontalMatch(_PositionToCheck, _GemToCheck) || 
               CheckVerticalMatch(_PositionToCheck, _GemToCheck);
    }

    private bool CheckHorizontalMatch(Vector2Int pos, SC_Gem gemToCheck)
    {
        int leftCount = CountMatchingGemsInDirection(pos, -1, 0, gemToCheck.type);
        int rightCount = CountMatchingGemsInDirection(pos, 1, 0, gemToCheck.type);
        
        return (leftCount + rightCount) >= 2;
    }

    private bool CheckVerticalMatch(Vector2Int pos, SC_Gem gemToCheck)
    {
        int belowCount = CountMatchingGemsInDirection(pos, 0, -1, gemToCheck.type);
        int aboveCount = CountMatchingGemsInDirection(pos, 0, 1, gemToCheck.type);
        
        return (belowCount + aboveCount) >= 2;
    }

    private int CountMatchingGemsInDirection(Vector2Int startPos, int deltaX, int deltaY, GlobalEnums.GemType typeToMatch)
    {
        int count = 0;
        int x = startPos.x + deltaX;
        int y = startPos.y + deltaY;

        while (IsValidPosition(x, y) && allGems[x, y] != null && allGems[x, y].type == typeToMatch)
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

    public void SetGem(int _X, int _Y, SC_Gem _Gem)
    {
        allGems[_X, _Y] = _Gem;
    }
    public SC_Gem GetGem(int _X,int _Y)
    {
       return allGems[_X, _Y];
    }

    public void FindAllMatches(Vector2Int? userActionPos = null)
    {
        explosions.Clear();
        foreach (var matchInfo in matchInfoMap)
        {
            if (matchInfo.MatchedGems != null)
                HashSetPool<SC_Gem>.Release(matchInfo.MatchedGems);
        }
        matchInfoMap.Clear();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                SC_Gem currentGem = allGems[x, y];
                if (currentGem != null)
                {
                    HashSet<SC_Gem> horizontalMatches = CheckMatchesInDirection(x, y, 1, 0);
                    HashSet<SC_Gem> verticalMatches = CheckMatchesInDirection(x, y, 0, 1);

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
                    HashSetPool<SC_Gem>.Release(newMatch.MatchedGems);
                return;
            }
        }
        
        matchInfoMap.Add(newMatch);
    }

    private HashSet<SC_Gem> CheckMatchesInDirection(int x, int y, int deltaX, int deltaY)
    {
        SC_Gem currentGem = allGems[x, y];
        
        if (!currentGem)
            return null;

        var matches = HashSetPool<SC_Gem>.Get();
        matches.Add(currentGem);

        foreach (var gem in GetMatchingGemsInDirection(x, y, deltaX, deltaY, currentGem.type))
        {
            matches.Add(gem);
        }

        foreach (var gem in GetMatchingGemsInDirection(x, y, -deltaX, -deltaY, currentGem.type))
        {
            matches.Add(gem);
        }

        if (matches.Count < 3)
        {
            HashSetPool<SC_Gem>.Release(matches);
            return null;
        }

        return matches;
    }

    private IEnumerable<SC_Gem> GetMatchingGemsInDirection(int startX, int startY, int deltaX, int deltaY, GlobalEnums.GemType typeToMatch)
    {
        int x = startX + deltaX;
        int y = startY + deltaY;

        while (IsValidPosition(x, y) && allGems[x, y] && allGems[x, y].type == typeToMatch)
        {
            yield return allGems[x, y];
            x += deltaX;
            y += deltaY;
        }
    }

    public void CheckForBombs()
    {
        foreach (var matchInfo in MatchInfoMap)
        foreach (var gem in matchInfo.MatchedGems)
        {
            int x = gem.posIndex.x;
            int y = gem.posIndex.y;

            if (gem.posIndex.x > 0)
            {
                if (allGems[x - 1, y] != null && allGems[x - 1, y].type == GlobalEnums.GemType.bomb)
                    MarkBombArea(new Vector2Int(x - 1, y), allGems[x - 1, y].blastSize);
            }

            if (gem.posIndex.x + 1 < width)
            {
                if (allGems[x + 1, y] != null && allGems[x + 1, y].type == GlobalEnums.GemType.bomb)
                    MarkBombArea(new Vector2Int(x + 1, y), allGems[x + 1, y].blastSize);
            }

            if (gem.posIndex.y > 0)
            {
                if (allGems[x, y - 1] != null && allGems[x, y - 1].type == GlobalEnums.GemType.bomb)
                    MarkBombArea(new Vector2Int(x, y - 1), allGems[x, y - 1].blastSize);
            }

            if (gem.posIndex.y + 1 < height)
            {
                if (allGems[x, y + 1] != null && allGems[x, y + 1].type == GlobalEnums.GemType.bomb)
                    MarkBombArea(new Vector2Int(x, y + 1), allGems[x, y + 1].blastSize);
            }
        }
    }

    public void MarkBombArea(Vector2Int bombPos, int _BlastSize)
    {
        string _print = "";
        for (int x = bombPos.x - _BlastSize; x <= bombPos.x + _BlastSize; x++)
        {
            for (int y = bombPos.y - _BlastSize; y <= bombPos.y + _BlastSize; y++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    var gem = allGems[x, y];
                    
                    if (gem != null)
                    {
                        _print += "(" + x + "," + y + ")" + System.Environment.NewLine;
                        
                        MarkGemAsMatched(gem);
                        
                        explosions.Add(gem);
                    }
                }
            }
        }
    }
    
    public void MarkColorBombArea(Vector2Int bombPos, int _BlastSize)
    {
        string _print = "";
        for (int x = bombPos.x - _BlastSize; x <= bombPos.x + _BlastSize; x++)
        {
            for (int y = bombPos.y - _BlastSize; y <= bombPos.y + _BlastSize; y++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    var gem = allGems[x, y];

                    if (gem != null)
                    {
                        int dx = x - bombPos.x;
                        int dy = y - bombPos.y;
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);
                        if (distance > _BlastSize)
                        {
                            continue;
                        }
                        
                        _print += "(" + x + "," + y + ")" + System.Environment.NewLine;

                        MarkGemAsMatched(gem);
                        
                        explosions.Add(gem);
                    }
                }
            }
        }
    }
    
    private void MarkGemAsMatched(SC_Gem gem)
    {
        if (gem != null && gem.isMatch == false)
        {
            gem.isMatch = true;
            
            if (gem.isColorBomb)
            {
                MarkColorBombArea(gem.posIndex, gem.blastSize);
            }
        }
    }
}

