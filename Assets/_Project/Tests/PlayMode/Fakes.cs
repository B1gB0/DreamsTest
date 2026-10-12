using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Domain;
using Game.Services.Analytics;
using Game.Services.IAP;
using Game.Services.Storage;

namespace Game.Tests.PlayMode
{
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

    internal sealed class FakeAnalyticsProvider : IAnalyticsProvider
    {
        public List<string> Events { get; } = new();

        public void LogEvent(
            string eventName,
            Dictionary<string, object> parameters)
        {
            Events.Add(eventName);
        }
    }

    internal sealed class FakeStorageService : IStorageService
    {
        public SaveData Data { get; set; }

        public void Save<T>(string key, T data) where T : class
        {
            Data = data as SaveData;
        }

        public T Load<T>(string key) where T : class
        {
            return Data as T;
        }

        public bool Has(string key)
        {
            return Data != null;
        }

        public void Delete(string key)
        {
            Data = null;
        }
    }

    internal sealed class FakeIAPService : IIAPService
    {
        public bool IsAvailable => true;

        public event Action<string> OnPurchaseSuccess;
        public event Action<string, string> OnPurchaseFailed;

        public Task<bool> InitializeAsync()
        {
            return Task.FromResult(true);
        }

        public Task PurchaseAsync(string productId)
        {
            return Task.CompletedTask;
        }
        
        public void SimulatePurchaseSuccess(string productId)
        {
            OnPurchaseSuccess?.Invoke(productId);
        }

        public void SimulatePurchaseFailed(string productId, string reason)
        {
            OnPurchaseFailed?.Invoke(productId, reason);
        }
    }
}
