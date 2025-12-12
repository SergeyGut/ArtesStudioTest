using Domain.Interfaces;

namespace Domain
{
    public class MatchCounterService : IMatchCounterService
    {
        private readonly IGameBoard gameBoard;

        public MatchCounterService(IGameBoard gameBoard)
        {
            this.gameBoard = gameBoard;
        }

        public int GetMatchCountAt(GridPosition _PositionToCheck, IPiece gemToCheck)
        {
            int horizontalMatches = CountHorizontalMatch(_PositionToCheck, gemToCheck.Type);
            int verticalMatches = CountVerticalMatch(_PositionToCheck, gemToCheck.Type);
            return (horizontalMatches >= 2 ? horizontalMatches : 0) +
                   (verticalMatches >= 2 ? verticalMatches : 0);
        }

        private int CountHorizontalMatch(GridPosition pos, GemType typeToMatch)
        {
            int leftCount = CountMatchingGemsInDirection(pos, -1, 0, typeToMatch);
            int rightCount = CountMatchingGemsInDirection(pos, 1, 0, typeToMatch);

            return leftCount + rightCount;
        }

        private int CountVerticalMatch(GridPosition pos, GemType typeToMatch)
        {
            int belowCount = CountMatchingGemsInDirection(pos, 0, -1, typeToMatch);
            int aboveCount = CountMatchingGemsInDirection(pos, 0, 1, typeToMatch);

            return belowCount + aboveCount;
        }

        private int CountMatchingGemsInDirection(GridPosition startPos, int deltaX, int deltaY, GemType typeToMatch)
        {
            int count = 0;
            int x = startPos.X + deltaX;
            int y = startPos.Y + deltaY;

            while (gameBoard.IsValidPosition(x, y) && gameBoard.GetGem(x, y)?.Type == typeToMatch)
            {
                count++;
                x += deltaX;
                y += deltaY;
            }

            return count;
        }
    }
}