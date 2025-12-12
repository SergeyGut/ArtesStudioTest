using System.Collections.Generic;
using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface IDestroyService
    {
        void DestroyGems(IEnumerable<IPiece> gems);
    }
}