using System;
using Game.Domain;

namespace Game.Config
{
    [Serializable]
    public class MachineConfig : IMachineConfig
    {
        public string Id => id;
        public double UnlockCost => unlockCost;
        public double BaseProduction => baseProduction;
        public double BaseUpgradeCost => baseUpgradeCost;
        public double UpgradeMultiplier => upgradeMultiplier;
        public bool StartsUnlocked => startsUnlocked;

        [UnityEngine.SerializeField]
        private string id;

        [UnityEngine.SerializeField]
        private double unlockCost;

        [UnityEngine.SerializeField]
        private double baseProduction;

        [UnityEngine.SerializeField]
        private double baseUpgradeCost;

        [UnityEngine.SerializeField]
        private double upgradeMultiplier = 1.5d;

        [UnityEngine.SerializeField]
        private bool startsUnlocked;
    }
}
