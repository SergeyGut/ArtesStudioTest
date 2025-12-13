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
        private readonly IGemPool<IPieceView> gemPool;
        private readonly ISettings settings;

        private readonly Random random = new();

        public SpawnService(
            IGameBoard gameBoard,
            IDropService dropService,
            IBoardView gameBoardView,
            IMatchCounterService matchCounterService,
            IGemPool<IPieceView> gemPool,
            ISettings settings)
        {
            this.gameBoard = gameBoard;
            this.dropService = dropService;
            this.gameBoardView = gameBoardView;
            this.matchCounterService = matchCounterService;
            this.gemPool = gemPool;
            this.settings = settings;
        }
        
        public void Initialize()
        {
            for (int x = 0; x < gameBoard.Width; x++)
            for (int y = 0; y < gameBoard.Height; y++)
            {
                var pos = new GridPosition(x, y);
                var gemToSpawn = SelectNonMatchingGem(pos);
                SpawnGem(pos, gemToSpawn);
            }
        }

        private IPieceData SelectNonMatchingGem(GridPosition position)
        {
            using var validGems = PooledList<IPieceData>.Get();
            using var matchCounts = PooledList<int>.Get();
            int lowestMatchCount = int.MaxValue;
            var gems = settings.Gems;

            for (int i = 0; i < gems.Count; i++)
            {
                int matchCount = matchCounterService.GetMatchCountAt(position, gems[i].Type);
                matchCounts.Value.Add(matchCount);

                if (matchCount == 0)
                {
                    validGems.Value.Add(gems[i]);
                }
                else if (matchCount < lowestMatchCount)
                {
                    lowestMatchCount = matchCount;
                }
            }

            if (validGems.Value.Count > 0)
            {
                return validGems.Value[random.Next(0, validGems.Value.Count)];
            }

            validGems.Value.Clear();
            for (int i = 0; i < gems.Count; i++)
            {
                if (matchCounts.Value[i] == lowestMatchCount)
                {
                    validGems.Value.Add(gems[i]);
                }
            }

            return validGems.Value[random.Next(0, validGems.Value.Count)];
        }

        public void SpawnGem(GridPosition position, IPieceData pieceToSpawn, int dropHeight = 0)
        {
            if (random.Next(0, 100) < settings.BombChance)
            {
                pieceToSpawn = settings.Bomb;
            }

            position = new GridPosition(position.X, position.Y + dropHeight);
            
            var gem = new PieceModel(pieceToSpawn, position);
            var gemView = gemPool.SpawnGem(pieceToSpawn.PieceView, gem);
            
            gameBoardView.AddPieceView(gemView);
            
            if (dropHeight > 0)
            {
                dropService.RunDropAsync(gem);
            }
            else
            {
                gameBoard.SetGem(position, gem);
            }
        }

        public void SpawnTopX(int x)
        {
            GridPosition topPos = new GridPosition(x, gameBoard.Height - 1);
            IPiece topGem = gameBoard.GetGem(topPos);

            if (topGem == null)
            {
                var gemToSpawn = SelectNonMatchingGem(topPos);
                SpawnGem(topPos, gemToSpawn, settings.DropHeight);
            }
        }
    }
}

