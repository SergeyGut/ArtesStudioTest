using Domain.Interfaces;
using UnityEngine;

namespace Presentation.Settings
{
    [CreateAssetMenu(fileName = "GemData", menuName = "Game/GemData")]
    public class GemData : ScriptableObject, IPieceData
    {
        [SerializeField]
        private PieceType type;
        [SerializeField]
        private bool isColorBomb;
        [SerializeField]
        private int blastSize;
        [SerializeField]
        private int scoreValue;
        [SerializeField]
        private GemView gemViewPrefab;
        
        public PieceType Type => type;
        public IPieceView PieceView => gemViewPrefab;
        public bool IsColorBomb => isColorBomb;
        public int BlastSize => blastSize;
        public int ScoreValue => scoreValue;
    }
}

