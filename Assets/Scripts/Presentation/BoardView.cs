using System.Collections.Generic;
using Domain.Interfaces;
using Domain.Pool;
using Service.Interfaces;
using UnityEngine;
using Zenject;

namespace Presentation
{
    public class BoardView : IBoardView, IInitializable
    {
        private readonly IGameBoard gameBoard;
        private readonly ISettings settings;
        private readonly IGemPool<IPieceView> gemPool;
        private readonly Transform gemsHolder;

        private readonly Dictionary<IReadOnlyPiece, IPieceView> gemViews = new();
        
        public BoardView(
            [Inject(Id = "GemsHolder")] Transform gemsHolder,
            IGameBoard gameBoard,
            ISettings settings,
            IGemPool<IPieceView> gemPool)
        {
            this.gemsHolder = gemsHolder;
            this.gameBoard = gameBoard;
            this.settings = settings;
            this.gemPool = gemPool;
        }

        public void Initialize()
        {
            var parent = gemsHolder;

            for (int x = 0; x < gameBoard.Width; x++)
            for (int y = 0; y < gameBoard.Height; y++)
            {
                Vector2 _pos = new Vector2(x, y);
                GameObject _bgTile = Object.Instantiate(settings.TilePrefabs as GameObject, _pos, Quaternion.identity);
                _bgTile.transform.SetParent(parent);
                _bgTile.name = "BG Tile - " + x + ", " + y;
            }
        }

        public void AddPieceView(IPieceView pieceView)
        {
            gemViews.Add(pieceView.Piece, pieceView);
        }
        
        public IPieceView GetPieceView(IPiece piece)
        {
            return gemViews[piece];
        }

        public IPieceView RemovePieceView(IPiece piece)
        {
            if (gemViews.Remove(piece, out var pieceView))
            {
                return pieceView;
            }

            return null;
        }

        public void CheckMisplacedGems()
        {
            using var foundGems = PooledHashSet<IPieceView>.Get();
            foundGems.Value.UnionWith(Object.FindObjectsOfType<GemView>());

            for (int x = 0; x < gameBoard.Width; x++)
            {
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    IPiece gem = gameBoard.GetGem(x, y);
                    if (gem == null)
                    {
                        continue;
                    }
                    
                    IPieceView gemView = GetPieceView(gem);
                    if (gemView != null)
                    {
                        foundGems.Value.Remove(gemView);
                    }
                }
            }

            foreach (var pieceView in foundGems.Value)
            {
                gemPool.ReturnGem(pieceView);
            }
        }
    }
}