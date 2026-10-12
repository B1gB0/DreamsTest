using System;
using Game.Domain;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class OfflineProgressCalculatorTests
    {
        [Test]
        public void Calculate_ReturnsIncomeForTenMinutes()
        {
            DateTime now = DateTime.UtcNow;

            OfflineIncome income = OfflineProgressCalculator.Calculate(
                now.AddMinutes(-10),
                1d,
                3600);

            Assert.That(income.Amount, Is.EqualTo(600d));
            Assert.That(income.Seconds, Is.EqualTo(600));
        }

        [Test]
        public void Calculate_ClampsIncomeToMaximumOfflineTime()
        {
            DateTime now = DateTime.UtcNow;

            OfflineIncome income = OfflineProgressCalculator.Calculate(
                now.AddHours(-10),
                1d,
                3600);

            Assert.That(income.Amount, Is.EqualTo(3600d));
            Assert.That(income.Seconds, Is.EqualTo(3600));
        }

        [Test]
        public void Calculate_ReturnsZeroForFutureExitTime()
        {
            DateTime now = DateTime.UtcNow;

            OfflineIncome income = OfflineProgressCalculator.Calculate(
                now.AddMinutes(1),
                1d,
                3600);

            Assert.That(income.Amount, Is.EqualTo(0d));
            Assert.That(income.Seconds, Is.EqualTo(0));
        }

        [Test]
        public void Calculate_ReturnsZeroForZeroProduction()
        {
            OfflineIncome income = OfflineProgressCalculator.Calculate(
                DateTime.UtcNow.AddMinutes(-10),
                0d,
                3600);

            Assert.That(income.Amount, Is.EqualTo(0d));
            Assert.That(income.Seconds, Is.EqualTo(0));
        }
    }
}
