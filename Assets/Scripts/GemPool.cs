using UnityEngine;

public class GemPool : IGemPool
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

    public SC_Gem SpawnGem(SC_Gem prefab, GridPosition position, IGameLogic gameLogic, IGameBoard gameBoard, float dropHeight = 0f)
    {
        SC_Gem gem = pool.Get(prefab);
        
        gem.transform.position = new Vector3(position.X, position.Y + dropHeight, 0f);
        gem.transform.SetParent(parentTransform);
        gem.name = "Gem - " + position.X + ", " + position.Y;
        gem.SetupGem(gameLogic, gameBoard, position);

        return gem;
    }

    public void ReturnGem(SC_Gem gem)
    {
        if (gem != null)
            pool.Return(gem);
    }

    public void ClearPool()
    {
        pool.Clear();
    }
}

