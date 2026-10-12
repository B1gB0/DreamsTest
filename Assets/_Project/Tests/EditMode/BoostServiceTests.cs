using Game.Domain;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class BoostServiceTests
    {
        [Test]
        public void TryActivate_StartsBoostWithConfiguredMultiplier()
        {
            var boost = new BoostService(10f, 2f, true);

            bool result = boost.TryActivate();

            Assert.That(result, Is.True);
            Assert.That(boost.IsActive, Is.True);
            Assert.That(boost.GetCurrentMultiplier(), Is.EqualTo(2f));
        }

        [Test]
        public void TryActivate_WhenAlreadyActive_ReturnsFalse()
        {
            var boost = new BoostService(10f, 2f, true);
            boost.TryActivate();

            bool result = boost.TryActivate();

            Assert.That(result, Is.False);
        }

        [Test]
        public void Tick_ReducesRemainingTimeAndFinishesBoost()
        {
            var boost = new BoostService(10f, 2f, true);
            bool boostFinished = false;
            boost.BoostFinished += () => boostFinished = true;

            boost.TryActivate();
            boost.Tick(5d);

            Assert.That(boost.IsActive, Is.True);
            Assert.That(boost.RemainingSeconds, Is.EqualTo(5f));

            boost.Tick(6d);

            Assert.That(boost.IsActive, Is.False);
            Assert.That(boost.RemainingSeconds, Is.EqualTo(0f));
            Assert.That(boostFinished, Is.True);
        }

        [Test]
        public void TryActivate_WhenBoostDisabled_ReturnsFalse()
        {
            var boost = new BoostService(10f, 2f, false);

            bool result = boost.TryActivate();

            Assert.That(result, Is.False);
            Assert.That(boost.IsActive, Is.False);
        }
    }
}
