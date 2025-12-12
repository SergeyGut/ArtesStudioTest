using Domain.Interfaces;
using Service.Interfaces;
using UnityEngine;

namespace Presentation.Settings
{
    [CreateAssetMenu(fileName = "GemData", menuName = "Game/GemData")]
    public class GemData : ScriptableObject, IGemData
    {
        [SerializeField]
        private GemType type;
        [SerializeField]
        private bool isColorBomb;
        [SerializeField]
        private int blastSize;
        [SerializeField]
        private int scoreValue;
        [SerializeField]
        private SC_Gem gemViewPrefab;
        
        public GemType Type => type;
        public IPiece GemView => gemViewPrefab;
        public bool IsColorBomb => isColorBomb;
        public int BlastSize => blastSize;
        public int ScoreValue => scoreValue;
    }
}

