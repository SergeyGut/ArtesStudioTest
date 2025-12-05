using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard
{
    #region Variables

    private int height = 0;
    public int Height { get { return height; } }

    private int width = 0;
    public int Width { get { return width; } }
  
    private SC_Gem[,] allGems;
  //  public Gem[,] AllGems { get { return allGems; } }

    private int score = 0;
    public int Score 
    {
        get { return score; }
        set { score = value; }
    }

    private List<SC_Gem> currentMatches = new List<SC_Gem>();
    public List<SC_Gem> CurrentMatches { get { return currentMatches; } }
    
    private List<MatchInfo> matchInfoMap = new List<MatchInfo>();
    public List<MatchInfo> MatchInfoMap { get { return matchInfoMap; } }
    
    public class MatchInfo
    {
        public HashSet<SC_Gem> matchedGems;
        public Vector2Int? userActionPos;
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
        currentMatches.Clear();
        matchInfoMap.Clear();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                SC_Gem currentGem = allGems[x, y];
                if (currentGem != null)
                {
                    HashSet<SC_Gem> horizontalMatches = CheckHorizontalMatches(x, y);
                    HashSet<SC_Gem> verticalMatches = CheckVerticalMatches(x, y);

                    if (horizontalMatches != null)
                    {
                        AddMatch(new MatchInfo { matchedGems = horizontalMatches, userActionPos = userActionPos });
                    }

                    if (verticalMatches != null)
                    {
                        AddMatch(new MatchInfo { matchedGems = verticalMatches, userActionPos = userActionPos });
                    }
                }
            }

        if (currentMatches.Count > 0)
            currentMatches = currentMatches.Distinct().ToList();

        CheckForBombs();
    }

    private void AddMatch(MatchInfo newMatch)
    {
        currentMatches.AddRange(newMatch.matchedGems);

        foreach (var gem in newMatch.matchedGems)
        {
            MarkGemAsMatched(gem);
        }
        
        for (int i = 0; i < matchInfoMap.Count; ++i)
        {
            if (matchInfoMap[i].matchedGems.Overlaps(newMatch.matchedGems))
            {
                matchInfoMap[i].matchedGems.UnionWith(newMatch.matchedGems);
                matchInfoMap[i].userActionPos ??= newMatch.userActionPos;
                return;
            }
        }
        
        matchInfoMap.Add(newMatch);
    }
    
    private HashSet<SC_Gem> CheckHorizontalMatches(int x, int y)
    {
        SC_Gem currentGem = allGems[x, y];
        
        if (!currentGem)
            return null;

        HashSet<SC_Gem> matches = new HashSet<SC_Gem> { currentGem };

        int left = x - 1;
        while (left >= 0 && allGems[left, y] != null && allGems[left, y].type == currentGem.type)
        {
            matches.Add(allGems[left, y]);
            left--;
        }

        int right = x + 1;
        while (right < width && allGems[right, y] != null && allGems[right, y].type == currentGem.type)
        {
            matches.Add(allGems[right, y]);
            right++;
        }

        return matches.Count >= 3 ? matches : null;
    }

    private HashSet<SC_Gem> CheckVerticalMatches(int x, int y)
    {
        SC_Gem currentGem = allGems[x, y];
        
        if (!currentGem)
            return null;

        HashSet<SC_Gem> matches = new HashSet<SC_Gem> { currentGem };

        int below = y - 1;
        while (below >= 0 && allGems[x, below] != null && allGems[x, below].type == currentGem.type)
        {
            matches.Add(allGems[x, below]);
            below--;
        }

        int above = y + 1;
        while (above < height && allGems[x, above] != null && allGems[x, above].type == currentGem.type)
        {
            matches.Add(allGems[x, above]);
            above++;
        }

        return matches.Count >= 3 ? matches : null;
    }

    public void CheckForBombs()
    {
        for (int i = 0; i < currentMatches.Count; i++)
        {
            SC_Gem gem = currentMatches[i];
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
                        
                        currentMatches.Add(gem);
                    }
                }
            }
        }
        currentMatches = currentMatches.Distinct().ToList();
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
                        
                        currentMatches.Add(gem);
                    }
                }
            }
        }
        currentMatches = currentMatches.Distinct().ToList();
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

