using System;

namespace Game.Domain
{
    public class MachineModel
    {
        public string Id { get; private set; }
        public bool IsUnlocked { get; private set; }
        public int Level { get; private set; }
        public double ProductionPerSecond { get; private set; }
        public double UnlockCost { get; private set; }
        public double UpgradeCost { get; private set; }
        public double BaseUpgradeCost { get; private set; }
        public double UpgradeMultiplier { get; private set; }

        private readonly double _baseProductionPerSecond;

        public MachineModel(string id, double productionPerSecond, double unlockCost, 
            double baseUpgradeCost, double upgradeMultiplier)
        {
            Id = id;
            IsUnlocked = false;
            Level = 1;
            _baseProductionPerSecond = productionPerSecond;
            ProductionPerSecond = productionPerSecond;
            UnlockCost = unlockCost;
            BaseUpgradeCost = baseUpgradeCost;
            UpgradeMultiplier = upgradeMultiplier;
            UpgradeCost = CostCalculator.GetUpgradeCost(BaseUpgradeCost, Level, UpgradeMultiplier);
        }

        public MachineModel(IMachineConfig config, bool isUnlocked, int level)
            : this(
                config.Id,
                config.BaseProduction,
                config.UnlockCost,
                config.BaseUpgradeCost,
                config.UpgradeMultiplier)
        {
            IsUnlocked = isUnlocked;
            Level = Math.Max(1, level);
            ProductionPerSecond = _baseProductionPerSecond * Level;
            UpgradeCost = CostCalculator.GetUpgradeCost(BaseUpgradeCost, Level, UpgradeMultiplier);
        }

        public bool TryUnlock(ref double balance)
        {
            if (IsUnlocked || balance < UnlockCost)
                return false;

            balance -= UnlockCost;
            IsUnlocked = true;
            return true;
        }

        public bool TryUpgrade(ref double balance)
        {
            if (!IsUnlocked || balance < UpgradeCost)
                return false;

            balance -= UpgradeCost;
            ApplyUpgrade();
            return true;
        }

        public void ApplyUpgrade()
        {
            Level++;
            ProductionPerSecond = _baseProductionPerSecond * Level;
            UpgradeCost = CostCalculator.GetUpgradeCost(BaseUpgradeCost, Level, UpgradeMultiplier);
        }
    }
}
