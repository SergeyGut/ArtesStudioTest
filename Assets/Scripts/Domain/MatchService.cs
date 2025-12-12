using System.Collections.Generic;
using Domain.Interfaces;
using Domain.Pool;

namespace Domain
{
    public class MatchService : IMatchService
    {
        private readonly HashSet<IPiece> explosions = new();
        private readonly List<MatchInfo> matchInfoMap = new();

        private readonly IGameBoard gameBoard;

        public HashSet<IPiece> Explosions => explosions;
        public List<MatchInfo> MatchInfoMap => matchInfoMap;

        public MatchService(IGameBoard gameBoard)
        {
            this.gameBoard = gameBoard;
        }

        public void FindAllMatches(GridPosition? userActionPos = null, GridPosition? otherUserActionPos = null)
        {
            explosions.Clear();
            foreach (var matchInfo in matchInfoMap)
            {
                if (matchInfo.MatchedGems != null)
                    CollectionPool<HashSet<IPiece>>.Release(matchInfo.MatchedGems);
            }

            matchInfoMap.Clear();

            for (int x = 0; x < gameBoard.Width; x++)
            for (int y = 0; y < gameBoard.Height; y++)
            {
                IPiece currentGem = gameBoard.GetGem(x, y);
                if (currentGem != null)
                {
                    HashSet<IPiece> horizontalMatches = CheckMatchesInDirection(x, y, 1, 0);
                    HashSet<IPiece> verticalMatches = CheckMatchesInDirection(x, y, 0, 1);

                    if (horizontalMatches != null)
                    {
                        var pos = GetUserActionPosForMatch(horizontalMatches, userActionPos, otherUserActionPos);
                        AddMatch(new MatchInfo { MatchedGems = horizontalMatches, UserActionPos = pos });
                    }

                    if (verticalMatches != null)
                    {
                        var pos = GetUserActionPosForMatch(verticalMatches, userActionPos, otherUserActionPos);
                        AddMatch(new MatchInfo { MatchedGems = verticalMatches, UserActionPos = pos });
                    }
                }
            }

            CheckForBombs();
        }

        private GridPosition? GetUserActionPosForMatch(HashSet<IPiece> matchedGems, GridPosition? userActionPos = null,
            GridPosition? otherUserActionPos = null)
        {
            if (!userActionPos.HasValue || !otherUserActionPos.HasValue)
                return null;

            foreach (var matchedGem in matchedGems)
            {
                if (matchedGem.Position == userActionPos.Value)
                {
                    return userActionPos;
                }

                if (matchedGem.Position == otherUserActionPos.Value)
                {
                    return otherUserActionPos;
                }
            }

            return null;
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
                    {
                        CollectionPool<HashSet<IPiece>>.Release(newMatch.MatchedGems);
                    }

                    return;
                }
            }

            matchInfoMap.Add(newMatch);
        }

        private HashSet<IPiece> CheckMatchesInDirection(int x, int y, int deltaX, int deltaY)
        {
            IPiece currentGem = gameBoard.GetGem(x, y);

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

        private IEnumerable<IPiece> GetMatchingGemsInDirection(int startX, int startY, int deltaX, int deltaY,
            GemType typeToMatch)
        {
            int x = startX + deltaX;
            int y = startY + deltaY;

            while (gameBoard.IsValidPosition(x, y) && gameBoard.GetGem(x, y)?.Type == typeToMatch)
            {
                yield return gameBoard.GetGem(x, y);
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
                    var otherGem = gameBoard.GetGem(x - 1, y);
                    if (otherGem?.Type == GemType.bomb)
                        MarkBombArea(new GridPosition(x - 1, y), otherGem.BlastSize);
                }

                if (gem.Position.X + 1 < gameBoard.Width)
                {
                    var otherGem = gameBoard.GetGem(x + 1, y);
                    if (otherGem?.Type == GemType.bomb)
                        MarkBombArea(new GridPosition(x + 1, y), otherGem.BlastSize);
                }

                if (gem.Position.Y > 0)
                {
                    var otherGem = gameBoard.GetGem(x, y - 1);
                    if (otherGem?.Type == GemType.bomb)
                        MarkBombArea(new GridPosition(x, y - 1), otherGem.BlastSize);
                }

                if (gem.Position.Y + 1 < gameBoard.Height)
                {
                    var otherGem = gameBoard.GetGem(x, y + 1);
                    if (otherGem?.Type == GemType.bomb)
                        MarkBombArea(new GridPosition(x, y + 1), otherGem.BlastSize);
                }
            }
        }

        private void MarkBombArea(GridPosition bombPos, int _BlastSize)
        {
            for (int x = bombPos.X - _BlastSize; x <= bombPos.X + _BlastSize; x++)
            {
                for (int y = bombPos.Y - _BlastSize; y <= bombPos.Y + _BlastSize; y++)
                {
                    if (x >= 0 && x < gameBoard.Width && y >= 0 && y < gameBoard.Height)
                    {
                        var gem = gameBoard.GetGem(x, y);
                        if (gem == null) continue;

                        MarkGemAsMatched(gem);
                        explosions.Add(gem);
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
                    if (x >= 0 && x < gameBoard.Width && y >= 0 && y < gameBoard.Height)
                    {
                        var gem = gameBoard.GetGem(x, y);
                        if (gem == null) continue;

                        int dx = x - bombPos.X;
                        int dy = y - bombPos.Y;
                        int sqrDistance = dx * dx + dy * dy;
                        if (sqrDistance > sqrBlastSize) continue;

                        MarkGemAsMatched(gem);
                        explosions.Add(gem);
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
}