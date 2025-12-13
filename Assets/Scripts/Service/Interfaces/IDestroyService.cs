using System.Collections.Generic;
using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface IDestroyService
    {
        void DestroyMatchedPieces(IEnumerable<IPiece> pieces);
    }
}