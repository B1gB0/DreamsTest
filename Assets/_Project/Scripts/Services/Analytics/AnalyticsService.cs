using System.Collections.Generic;

namespace Game.Services.Analytics
{
    public class AnalyticsService
    {
        private readonly IAnalyticsProvider[] _providers;

        public AnalyticsService(params IAnalyticsProvider[] providers)
        {
            _providers = providers ?? new IAnalyticsProvider[0];
        }

        public void LogGameStarted()
        {
            Log("game_started", new Dictionary<string, object>());
        }

        public void LogMachineUnlocked(string id, double cost)
        {
            Log("machine_unlocked", new Dictionary<string, object>
            {
                ["id"] = id,
                ["cost"] = cost
            });
        }

        public void LogMachineUpgraded(string id, int newLevel, double cost)
        {
            Log("machine_upgraded", new Dictionary<string, object>
            {
                ["id"] = id,
                ["new_level"] = newLevel,
                ["cost"] = cost
            });
        }

        public void LogBoostStarted(float duration, float multiplier)
        {
            Log("boost_started", new Dictionary<string, object>
            {
                ["duration"] = duration,
                ["multiplier"] = multiplier
            });
        }

        public void LogBoostFinished()
        {
            Log("boost_finished", new Dictionary<string, object>());
        }

        public void LogOfflineIncome(double amount, int seconds)
        {
            Log("offline_income_applied", new Dictionary<string, object>
            {
                ["amount"] = amount,
                ["seconds"] = seconds
            });
        }

        public void LogPurchaseSucceeded(string productId)
        {
            Log("purchase_succeeded", new Dictionary<string, object>
            {
                ["product_id"] = productId
            });
        }

        public void LogPurchaseFailed(string productId, string reason)
        {
            Log("purchase_failed", new Dictionary<string, object>
            {
                ["product_id"] = productId,
                ["reason"] = reason
            });
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            Log(eventName, parameters ?? new Dictionary<string, object>());
        }

        private void Log(string eventName, Dictionary<string, object> parameters)
        {
            foreach (IAnalyticsProvider provider in _providers)
            {
                provider.LogEvent(eventName, parameters);
            }
        }
    }
}
