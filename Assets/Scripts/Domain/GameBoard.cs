using System;
using Domain.Interfaces;

namespace Domain
{
    public class GameBoard : IGameBoard
    {
        private readonly int height;
        private readonly int width;
        private readonly IPiece[,] allPieces;

        public int Width => width;
        public int Height => height;

        public GameBoard(IBoardSettings settings)
        {
            height = settings.RowsSize;
            width = settings.ColsSize;
            allPieces = new IPiece[width, height];
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public void SetPiece(int x, int y, IPiece piece)
        {
            if (!IsValidPosition(x, y))
            {
                throw new ArgumentException($"SetPiece: Position ({x}, {y}) is out of bounds.");
            }

            allPieces[x, y] = piece;
        }

        public IPiece GetPiece(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                throw new ArgumentException($"GetPiece: Position ({x}, {y}) is out of bounds.");
            }

            return allPieces[x, y];
        }

        public void SetPiece(GridPosition position, IPiece piece)
        {
            SetPiece(position.X, position.Y, piece);
        }

        public IPiece GetPiece(GridPosition position)
        {
            return GetPiece(position.X, position.Y);
        }
    }
}
