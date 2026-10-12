using System.Collections;
using System.Collections.Generic;
using Game.Domain;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public class EconomyServicePlayModeTests
    {
        [UnityTest]
        public IEnumerator EconomyService_TickProductionOverTime()
        {
            var machine = new FakeMachineConfig(
                "machine_1",
                0d,
                10d,
                10d,
                1.5d,
                true);
            var config = new FakeConfig(machine);
            var economy = new EconomyService(config, config.Machines);

            for (int index = 0; index < 10; index++)
            {
                economy.TryTickProduction(0.1d);
                yield return null;
            }

            Assert.That(economy.Balance, Is.EqualTo(10d).Within(0.0001d));
        }
    }
}
