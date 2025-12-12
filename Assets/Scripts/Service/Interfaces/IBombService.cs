using System.Collections.Generic;
using Domain;
using Domain.Interfaces;
using Domain.Pool;

namespace Service.Interfaces
{
    public interface IBombService
    {
        void CreateBombs(Dictionary<GridPosition, GemType> bombPositions, PooledHashSet<IPiece> newlyCreatedBombs);
    }
}