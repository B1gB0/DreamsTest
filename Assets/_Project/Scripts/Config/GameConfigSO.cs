using System.Collections.Generic;
using Game.Domain;
using UnityEngine;

namespace Game.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Mini Factory/Game Config")]
    public class GameConfigSO : ScriptableObject, IGameConfig
    {
        public IReadOnlyList<IMachineConfig> Machines => machines;
        public float BoostDurationSeconds => boostDurationSeconds;
        public float BoostMultiplier => boostMultiplier;
        public bool BoostEnabled => boostEnabled;
        public int MaxOfflineSeconds => maxOfflineSeconds;
        public int CoinsPackAmount => coinsPackAmount;
        public string CoinsPackProductId => coinsPackProductId;

        [SerializeField]
        private MachineConfig[] machines;

        [SerializeField]
        private float boostDurationSeconds = 30f;

        [SerializeField]
        private float boostMultiplier = 2f;

        [SerializeField]
        private bool boostEnabled = true;

        [SerializeField]
        private int maxOfflineSeconds = 3600;

        [SerializeField]
        private int coinsPackAmount = 1000;

        [SerializeField]
        private string coinsPackProductId = "coins_pack_small";
    }
}
