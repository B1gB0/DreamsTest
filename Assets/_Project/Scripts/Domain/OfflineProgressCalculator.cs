using System;

namespace Game.Domain
{
    public readonly struct OfflineIncome
    {
        public readonly double Amount;
        public readonly int Seconds;

        public OfflineIncome(double amount, int seconds)
        {
            Amount = amount;
            Seconds = seconds;
        }
    }

    public static class OfflineProgressCalculator
    {
        public static OfflineIncome Calculate(
            DateTime lastExitUtc,
            double productionPerSecond,
            int maxOfflineSeconds)
        {
            int seconds = (int)(DateTime.UtcNow - lastExitUtc).TotalSeconds;

            if (seconds <= 0 || productionPerSecond <= 0 || maxOfflineSeconds <= 0)
            {
                return new OfflineIncome(0, 0);
            }

            seconds = Math.Min(seconds, maxOfflineSeconds);
            return new OfflineIncome(seconds * productionPerSecond, seconds);
        }
    }
}
