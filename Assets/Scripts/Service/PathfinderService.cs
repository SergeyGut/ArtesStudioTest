using Domain;
using Domain.Interfaces;
using Domain.Pool;
using Service.Interfaces;

namespace Service
{
    public class PathfinderService : IPathfinderService
    {
        private readonly IMatchService matchService;
        private readonly ISettings settings;

        public PathfinderService(
            IMatchService matchService,
            ISettings settings)
        {
            this.matchService = matchService;
            this.settings = settings;
        }

        public PooledDictionary<GridPosition, PieceType> CollectBombCreationPositions()
        {
            PooledDictionary<GridPosition, PieceType> bombCreationPositions = PooledDictionary<GridPosition, PieceType>.Get();

            foreach (MatchInfo matchInfo in matchService.MatchInfoMap)
            {
                if (matchInfo.MatchedGems.Count < settings.MinMatchForBomb)
                {
                    continue;
                }
                
                IPiece firstGem = null;
                foreach (IPiece gem in matchInfo.MatchedGems)
                {
                    firstGem = gem;
                    break;
                }

                if (firstGem != null)
                {
                    bombCreationPositions.Value.TryAdd(matchInfo.UserActionPos ?? firstGem.Position, firstGem.Type);
                }
            }

            return bombCreationPositions;
        }

        public PooledHashSet<IPiece> CollectMatchedGems()
        {
            PooledHashSet<IPiece> matchedGems = PooledHashSet<IPiece>.Get();
            
            foreach (MatchInfo matchInfo in matchService.MatchInfoMap)
            {
                foreach (IPiece gem in matchInfo.MatchedGems)
                {
                    if (gem is { IsColorBomb: false } && gem.Type != PieceType.bomb)
                    {
                        matchedGems.Value.Add(gem);
                    }
                }
            }

            return matchedGems;
        }

        public PooledList<IPiece> CollectExplosionsNonBomb(PooledHashSet<IPiece> matchedGems)
        {
            PooledList<IPiece> nonBombExplosions = PooledList<IPiece>.Get();
            
            foreach (IPiece gem in matchService.Explosions)
            {
                if (gem is { IsColorBomb: false } && gem.Type != PieceType.bomb && !matchedGems.Value.Contains(gem))
                {
                    nonBombExplosions.Value.Add(gem);
                }
            }

            return nonBombExplosions;
        }

        public PooledList<IPiece> CollectExplosionsBomb(PooledHashSet<IPiece> matchedGems)
        {
            PooledList<IPiece> bombExplosions = PooledList<IPiece>.Get();
            
            foreach (IPiece gem in matchService.Explosions)
            {
                if (gem != null && (gem.IsColorBomb || gem.Type == PieceType.bomb) && !matchedGems.Value.Contains(gem))
                {
                    bombExplosions.Value.Add(gem);
                }
            }

            return bombExplosions;
        }
    }
}
