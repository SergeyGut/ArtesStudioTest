using System.Collections.Generic;
using Domain.Interfaces;

namespace Domain
{
    public class MatchInfo
    {
        public HashSet<IPiece> MatchedGems;
        public GridPosition? UserActionPos;
    }
}