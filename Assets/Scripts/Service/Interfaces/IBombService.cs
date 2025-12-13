using System.Collections.Generic;
using Domain;
using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface IBombService
    {
        void CreateBombs(Dictionary<GridPosition, PieceType> bombPositions);
    }
}