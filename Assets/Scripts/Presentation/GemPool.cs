using System;
using Domain.Interfaces;
using Presentation.Pool;
using Service.Interfaces;
using UnityEngine;
using Zenject;

namespace Presentation
{
    public class GemPool : IGemPool<IPieceView>, IDisposable
    {
        private readonly IObjectPool<GemView> pool;
        private readonly Transform parentTransform;

        public int AvailableCount => pool.AvailableCount;
        public int ActiveCount => pool.ActiveCount;

        public GemPool([Inject(Id = "GemsHolder")] Transform parent, DiContainer container)
        {
            parentTransform = parent;
            pool = new GenericObjectPool<GemView>(parent, container);
        }

        public IPieceView SpawnGem(IPieceView item, IPiece piece)
        {
            if (item is not GemView prefab)
                return null;

            GemView gem = pool.Get(prefab);

            gem.transform.SetParent(parentTransform);
            gem.transform.position = new Vector3(piece.Position.X, piece.Position.Y, 0f);
            gem.name = "Gem - " + piece.Position.X + ", " + piece.Position.Y;
            gem.SetupGem(piece);

            return gem;
        }

        public void ReturnGem(IPieceView item)
        {
            if (item is GemView gem)
                pool.Return(gem);
        }

        public void ClearPool()
        {
            pool.Clear();
        }

        public void Dispose()
        {
            ClearPool();
        }
    }
}
