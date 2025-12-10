using System.Collections.Generic;

public interface IDestroyService
{
    void DestroyGems(IEnumerable<IPiece> gems);
}
