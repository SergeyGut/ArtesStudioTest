using System.Collections.Generic;
using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface IDestroyService
    {
        void DestroyMatchedGems(IEnumerable<IPiece> gems);
    }
}