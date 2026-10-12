using Game.Domain;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class CostCalculatorTests
    {
        [TestCase(10d, 0, 1.5d, 10d)]
        [TestCase(10d, 1, 1.5d, 15d)]
        [TestCase(10d, 2, 1.5d, 22.5d)]
        public void GetUpgradeCost_ReturnsExpectedCost(
            double baseCost,
            int level,
            double multiplier,
            double expectedCost)
        {
            double actualCost = CostCalculator.GetUpgradeCost(
                baseCost,
                level,
                multiplier);

            Assert.That(actualCost, Is.EqualTo(expectedCost).Within(0.000001d));
        }

        [TestCase(100d, 100d)]
        [TestCase(0d, 0d)]
        public void GetUnlockCost_ReturnsBaseCost(double baseCost, double expectedCost)
        {
            double actualCost = CostCalculator.GetUnlockCost(baseCost);

            Assert.That(actualCost, Is.EqualTo(expectedCost));
        }
    }
}
