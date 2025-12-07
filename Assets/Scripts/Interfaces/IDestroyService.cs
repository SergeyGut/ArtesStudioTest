using System.Collections.Generic;

public interface IDestroyService
{
    void DestroyGems(IEnumerable<SC_Gem> gems);
}
