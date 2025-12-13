using Cysharp.Threading.Tasks;
using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface IDropService
    {
        bool DropSingleX(int x);
        UniTask RunDropAsync(IPiece gem);
    }
}