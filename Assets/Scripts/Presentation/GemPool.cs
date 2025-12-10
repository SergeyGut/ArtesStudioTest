using UnityEngine;

public class GemPool : IGemPool<IPiece>
{
    private readonly IObjectPool<SC_Gem> pool;
    private readonly Transform parentTransform;

    public int AvailableCount => pool.AvailableCount;
    public int ActiveCount => pool.ActiveCount;

    public GemPool(Transform parent)
    {
        parentTransform = parent;
        pool = new GenericObjectPool<SC_Gem>(parent);
    }

    public IPiece SpawnGem(IPiece item, GridPosition position, IGameLogic gameLogic, IGameBoard gameBoard, float dropHeight = 0f)
    {
        if (item is not SC_Gem prefab)
            return null;

        SC_Gem gem = pool.Get(prefab);
        
        gem.transform.position = new Vector3(position.X, position.Y + dropHeight, 0f);
        gem.transform.SetParent(parentTransform);
        gem.name = "Gem - " + position.X + ", " + position.Y;
        gem.SetupGem(gameLogic, gameBoard, position);

        return gem;
    }

    public void ReturnGem(IPiece item)
    {
        if (item is SC_Gem gem)
            pool.Return(gem);
    }

    public void ClearPool()
    {
        pool.Clear();
    }
}

