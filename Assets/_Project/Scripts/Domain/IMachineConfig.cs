namespace Game.Domain
{
    public interface IMachineConfig
    {
        string Id { get; }
        double UnlockCost { get; }
        double BaseProduction { get; }
        double BaseUpgradeCost { get; }
        double UpgradeMultiplier { get; }
        bool StartsUnlocked { get; }
    }
}
