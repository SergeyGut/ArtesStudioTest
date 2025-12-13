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
                if (matchInfo.MatchedPieces != null)
                    CollectionPool<HashSet<IPiece>>.Release(matchInfo.MatchedPieces);
            }

            matchInfoMap.Clear();

            for (int x = 0; x < gameBoard.Width; x++)
            for (int y = 0; y < gameBoard.Height; y++)
            {
                IPiece currentPiece = gameBoard.GetPiece(x, y);
                if (currentPiece != null)
                {
                    HashSet<IPiece> horizontalMatches = CheckMatchesInDirection(x, y, 1, 0);
                    HashSet<IPiece> verticalMatches = CheckMatchesInDirection(x, y, 0, 1);

                    if (horizontalMatches != null)
                    {
                        var pos = GetUserActionPosForMatch(horizontalMatches, userActionPos, otherUserActionPos);
                        AddMatch(new MatchInfo { MatchedPieces = horizontalMatches, UserActionPos = pos });
                    }

                    if (verticalMatches != null)
                    {
                        var pos = GetUserActionPosForMatch(verticalMatches, userActionPos, otherUserActionPos);
                        AddMatch(new MatchInfo { MatchedPieces = verticalMatches, UserActionPos = pos });
                    }
                }
            }

            CheckForBombs();
        }

        private GridPosition? GetUserActionPosForMatch(HashSet<IPiece> matchedPieces, GridPosition? userActionPos = null,
            GridPosition? otherUserActionPos = null)
        {
            if (!userActionPos.HasValue || !otherUserActionPos.HasValue)
                return null;

            foreach (var matchedPiece in matchedPieces)
            {
                if (matchedPiece.Position == userActionPos.Value)
                {
                    return userActionPos;
                }

                if (matchedPiece.Position == otherUserActionPos.Value)
                {
                    return otherUserActionPos;
                }
            }

            return null;
        }

        private void AddMatch(MatchInfo newMatch)
        {
            foreach (var piece in newMatch.MatchedPieces)
            {
                MarkPieceAsMatched(piece);
            }

            for (int i = 0; i < matchInfoMap.Count; ++i)
            {
                if (matchInfoMap[i].MatchedPieces.Overlaps(newMatch.MatchedPieces))
                {
                    matchInfoMap[i].MatchedPieces.UnionWith(newMatch.MatchedPieces);
                    matchInfoMap[i].UserActionPos ??= newMatch.UserActionPos;
                    if (newMatch.MatchedPieces != matchInfoMap[i].MatchedPieces)
                    {
                        CollectionPool<HashSet<IPiece>>.Release(newMatch.MatchedPieces);
                    }

                    return;
                }
            }

            matchInfoMap.Add(newMatch);
        }

        private HashSet<IPiece> CheckMatchesInDirection(int x, int y, int deltaX, int deltaY)
        {
            IPiece currentPiece = gameBoard.GetPiece(x, y);

            if (currentPiece == null)
                return null;

            var matches = CollectionPool<HashSet<IPiece>>.Get();
            matches.Add(currentPiece);

            foreach (var piece in GetMatchingPiecesInDirection(x, y, deltaX, deltaY, currentPiece.Type))
            {
                matches.Add(piece);
            }

            foreach (var piece in GetMatchingPiecesInDirection(x, y, -deltaX, -deltaY, currentPiece.Type))
            {
                matches.Add(piece);
            }

            if (matches.Count < 3)
            {
                CollectionPool<HashSet<IPiece>>.Release(matches);
                return null;
            }

            return matches;
        }

        private IEnumerable<IPiece> GetMatchingPiecesInDirection(int startX, int startY, int deltaX, int deltaY,
            PieceType typeToMatch)
        {
            int x = startX + deltaX;
            int y = startY + deltaY;

            while (gameBoard.IsValidPosition(x, y) && gameBoard.GetPiece(x, y)?.Type == typeToMatch)
            {
                yield return gameBoard.GetPiece(x, y);
                x += deltaX;
                y += deltaY;
            }
        }

        private void CheckForBombs()
        {
            foreach (var matchInfo in MatchInfoMap)
            foreach (var piece in matchInfo.MatchedPieces)
            {
                int x = piece.Position.X;
                int y = piece.Position.Y;

                if (piece.Position.X > 0)
                {
                    var otherPiece = gameBoard.GetPiece(x - 1, y);
                    if (otherPiece?.Type == PieceType.bomb)
                        MarkBombArea(new GridPosition(x - 1, y), otherPiece.BlastSize);
                }

                if (piece.Position.X + 1 < gameBoard.Width)
                {
                    var otherPiece = gameBoard.GetPiece(x + 1, y);
                    if (otherPiece?.Type == PieceType.bomb)
                        MarkBombArea(new GridPosition(x + 1, y), otherPiece.BlastSize);
                }

                if (piece.Position.Y > 0)
                {
                    var otherPiece = gameBoard.GetPiece(x, y - 1);
                    if (otherPiece?.Type == PieceType.bomb)
                        MarkBombArea(new GridPosition(x, y - 1), otherPiece.BlastSize);
                }

                if (piece.Position.Y + 1 < gameBoard.Height)
                {
                    var otherPiece = gameBoard.GetPiece(x, y + 1);
                    if (otherPiece?.Type == PieceType.bomb)
                        MarkBombArea(new GridPosition(x, y + 1), otherPiece.BlastSize);
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
                        var piece = gameBoard.GetPiece(x, y);
                        if (piece == null) continue;

                        MarkPieceAsMatched(piece);
                        explosions.Add(piece);
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
                        var piece = gameBoard.GetPiece(x, y);
                        if (piece == null) continue;

                        int dx = x - bombPos.X;
                        int dy = y - bombPos.Y;
                        int sqrDistance = dx * dx + dy * dy;
                        if (sqrDistance > sqrBlastSize) continue;

                        MarkPieceAsMatched(piece);
                        explosions.Add(piece);
                    }
                }
            }
        }

        private void MarkPieceAsMatched(IPiece piece)
        {
            if (piece is { IsMatch: false })
            {
                piece.IsMatch = true;

                if (piece.IsColorBomb)
                {
                    MarkColorBombArea(piece.Position, piece.BlastSize);
                    return;
                }

                if (piece.Type == PieceType.bomb)
                {
                    MarkBombArea(piece.Position, piece.BlastSize);
                }
            }
        }
    }
}