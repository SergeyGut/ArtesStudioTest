using System;
using Domain;
using Domain.Interfaces;
using Domain.Pool;
using Service.Interfaces;
using Zenject;

namespace Service
{
    public class SpawnService : ISpawnService, IInitializable
    {
        private readonly IGameBoard gameBoard;
        private readonly IDropService dropService;
        private readonly IBoardView gameBoardView;
        private readonly IMatchCounterService matchCounterService;
        private readonly IPiecePool<IPieceView> piecePool;
        private readonly ISettings settings;

        private readonly Random random = new();

        public SpawnService(
            IGameBoard gameBoard,
            IDropService dropService,
            IBoardView gameBoardView,
            IMatchCounterService matchCounterService,
            IPiecePool<IPieceView> piecePool,
            ISettings settings)
        {
            this.gameBoard = gameBoard;
            this.dropService = dropService;
            this.gameBoardView = gameBoardView;
            this.matchCounterService = matchCounterService;
            this.piecePool = piecePool;
            this.settings = settings;
        }
        
        public void Initialize()
        {
            for (int x = 0; x < gameBoard.Width; x++)
            for (int y = 0; y < gameBoard.Height; y++)
            {
                var pos = new GridPosition(x, y);
                var pieceToSpawn = SelectNonMatchingPiece(pos);
                SpawnPiece(pos, pieceToSpawn);
            }
        }

        private IPieceData SelectNonMatchingPiece(GridPosition position)
        {
            using var validPieces = PooledList<IPieceData>.Get();
            using var matchCounts = PooledList<int>.Get();
            int lowestMatchCount = int.MaxValue;
            var pieces = settings.Pieces;

            for (int i = 0; i < pieces.Count; i++)
            {
                int matchCount = matchCounterService.GetMatchCountAt(position, pieces[i].Type);
                matchCounts.Value.Add(matchCount);

                if (matchCount == 0)
                {
                    validPieces.Value.Add(pieces[i]);
                }
                else if (matchCount < lowestMatchCount)
                {
                    lowestMatchCount = matchCount;
                }
            }

            if (validPieces.Value.Count > 0)
            {
                return validPieces.Value[random.Next(0, validPieces.Value.Count)];
            }

            validPieces.Value.Clear();
            for (int i = 0; i < pieces.Count; i++)
            {
                if (matchCounts.Value[i] == lowestMatchCount)
                {
                    validPieces.Value.Add(pieces[i]);
                }
            }

            return validPieces.Value[random.Next(0, validPieces.Value.Count)];
        }

        public void SpawnPiece(GridPosition position, IPieceData pieceToSpawn, int dropHeight = 0)
        {
            if (random.Next(0, 100) < settings.BombChance)
            {
                pieceToSpawn = settings.Bomb;
            }

            position = new GridPosition(position.X, position.Y + dropHeight);
            
            var piece = new PieceModel(pieceToSpawn, position);
            var pieceView = piecePool.SpawnPiece(pieceToSpawn.PieceView, piece);
            
            gameBoardView.AddPieceView(pieceView);
            
            if (dropHeight > 0)
            {
                dropService.RunDropAsync(piece);
            }
            else
            {
                gameBoard.SetPiece(position, piece);
            }
        }

        public void SpawnTopX(int x)
        {
            GridPosition topPos = new GridPosition(x, gameBoard.Height - 1);
            IPiece topPiece = gameBoard.GetPiece(topPos);

            if (topPiece == null)
            {
                var pieceToSpawn = SelectNonMatchingPiece(topPos);
                SpawnPiece(topPos, pieceToSpawn, settings.DropHeight);
            }
        }
    }
}

