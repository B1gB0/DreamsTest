using System.Collections.Generic;
using Game.Domain;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class EconomyServiceTests
    {
        [Test]
        public void TryUnlockMachine_WhenBalanceIsInsufficient_ReturnsFalse()
        {
            EconomyService economy = CreateEconomy();
            economy.TryAddCoins(50d);

            bool result = economy.TryUnlockMachine("machine_1");

            Assert.That(result, Is.False);
            Assert.That(economy.Balance, Is.EqualTo(50d));
            Assert.That(economy.Machines[0].IsUnlocked, Is.False);
        }

        [Test]
        public void TryUnlockMachine_WhenBalanceIsSufficient_UnlocksMachine()
        {
            EconomyService economy = CreateEconomy();
            economy.TryAddCoins(150d);

            bool result = economy.TryUnlockMachine("machine_1");

            Assert.That(result, Is.True);
            Assert.That(economy.Balance, Is.EqualTo(50d));
            Assert.That(economy.Machines[0].IsUnlocked, Is.True);
        }

        [Test]
        public void TryUpgradeMachine_IncreasesLevelAndProduction()
        {
            EconomyService economy = CreateEconomy();
            economy.TryAddCoins(150d);
            economy.TryUnlockMachine("machine_1");
            economy.TryAddCoins(50d);

            double productionBeforeUpgrade =
                economy.Machines[0].ProductionPerSecond;

            bool result = economy.TryUpgradeMachine("machine_1");

            Assert.That(result, Is.True);
            Assert.That(economy.Balance, Is.EqualTo(85d));
            Assert.That(economy.Machines[0].Level, Is.EqualTo(2));
            Assert.That(
                economy.Machines[0].ProductionPerSecond,
                Is.GreaterThan(productionBeforeUpgrade));
        }

        [Test]
        public void TryTickProduction_AddsProductionToBalance()
        {
            var config = new FakeConfig(new FakeMachineConfig(
                "machine_1",
                0d,
                5d,
                10d,
                1.5d,
                true));
            var economy = new EconomyService(config, config.Machines);

            bool result = economy.TryTickProduction(1d);

            Assert.That(result, Is.True);
            Assert.That(economy.TotalProductionPerSecond, Is.EqualTo(5d));
            Assert.That(economy.Balance, Is.EqualTo(5d));
        }

        [Test]
        public void Economy_RaisesBalanceAndMachineUnlockedEvents()
        {
            EconomyService economy = CreateEconomy();
            bool balanceChanged = false;
            bool machineUnlocked = false;

            economy.BalanceChanged += balance => balanceChanged = true;
            economy.MachineUnlocked += machine => machineUnlocked = true;

            economy.TryAddCoins(150d);
            economy.TryUnlockMachine("machine_1");

            Assert.That(balanceChanged, Is.True);
            Assert.That(machineUnlocked, Is.True);
        }

        private static EconomyService CreateEconomy()
        {
            var config = new FakeConfig(new FakeMachineConfig(
                "machine_1",
                100d,
                1d,
                10d,
                1.5d,
                false));

            return new EconomyService(config, config.Machines);
        }
    }

    internal sealed class FakeConfig : IGameConfig
    {
        public FakeConfig(IMachineConfig machine)
        {
            Machines = new[] { machine };
        }

        public IReadOnlyList<IMachineConfig> Machines { get; }
        public float BoostDurationSeconds => 10f;
        public float BoostMultiplier => 2f;
        public bool BoostEnabled => true;
        public int MaxOfflineSeconds => 3600;
        public int CoinsPackAmount => 1000;
        public string CoinsPackProductId => "coins_pack_small";
    }

    internal sealed class FakeMachineConfig : IMachineConfig
    {
        public FakeMachineConfig(
            string id,
            double unlockCost,
            double baseProduction,
            double baseUpgradeCost,
            double upgradeMultiplier,
            bool startsUnlocked)
        {
            Id = id;
            UnlockCost = unlockCost;
            BaseProduction = baseProduction;
            BaseUpgradeCost = baseUpgradeCost;
            UpgradeMultiplier = upgradeMultiplier;
            StartsUnlocked = startsUnlocked;
        }

        public string Id { get; }
        public double UnlockCost { get; }
        public double BaseProduction { get; }
        public double BaseUpgradeCost { get; }
        public double UpgradeMultiplier { get; }
        public bool StartsUnlocked { get; }
    }
}
