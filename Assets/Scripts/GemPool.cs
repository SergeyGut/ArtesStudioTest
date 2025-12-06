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

    public SC_Gem SpawnGem(SC_Gem prefab, Vector2Int position, IGameLogic gameLogic, float dropHeight = 0f)
    {
        SC_Gem gem = pool.Get(prefab);
        
        gem.transform.position = new Vector3(position.x, position.y + dropHeight, 0f);
        gem.transform.SetParent(parentTransform);
        gem.name = "Gem - " + position.x + ", " + position.y;
        gem.SetupGem(gameLogic, position);

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

