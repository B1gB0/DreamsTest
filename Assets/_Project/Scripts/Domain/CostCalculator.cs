using System;

namespace Game.Domain
{
    public static class CostCalculator
    {
        public static double GetUpgradeCost(double baseCost, int level, double multiplier)
        {
            return baseCost * Math.Pow(multiplier, level);
        }

        public static double GetUnlockCost(double baseCost)
        {
            return baseCost;
        }
    }
}
