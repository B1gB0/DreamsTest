using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Game.Config;
using Game.Core;
using Game.Domain;
using Game.Presentation;
using Game.Services.Analytics;
using Game.Services.IAP;
using Game.Services.Storage;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public class GameBootstrapPlayModeTests
    {
        private readonly List<UnityEngine.Object> _createdObjects = new();

        [UnityTest]
        public IEnumerator GameBootstrap_InitializesServicesAndStartsGame()
        {
            var analytics = new FakeAnalyticsProvider();
            var config = CreateConfig(
                production: 1d,
                startsUnlocked: true,
                maxOfflineSeconds: 3600);
            var bootstrap = CreateBootstrap(
                config,
                new FakeStorageService(),
                new FakeIAPService(),
                analytics);

            yield return WaitForBootstrapInitialization(bootstrap);

            EconomyService economy = GetPrivateField<EconomyService>(
                bootstrap,
                "_economy");

            Assert.That(economy.Balance, Is.EqualTo(0d).Within(0.1d));
            Assert.That(economy.TotalProductionPerSecond, Is.EqualTo(1d));
            Assert.That(analytics.Events, Does.Contain("game_started"));
        }

        [UnityTest]
        public IEnumerator GameBootstrap_AppliesOfflineIncome()
        {
            var analytics = new FakeAnalyticsProvider();
            var storage = new FakeStorageService
            {
                Data = new SaveData
                {
                    Balance = 0d,
                    MachineLevels = new[] { 1 },
                    MachineUnlocked = new[] { true },
                    LastExitTimeUtc = DateTime.UtcNow
                        .AddHours(-1)
                        .ToString("o"),
                    LastBoostActivationUtc = string.Empty
                }
            };
            var config = CreateConfig(
                production: 1d,
                startsUnlocked: true,
                maxOfflineSeconds: 3600);
            var bootstrap = CreateBootstrap(
                config,
                storage,
                new FakeIAPService(),
                analytics);

            yield return WaitForBootstrapInitialization(bootstrap);

            EconomyService economy = GetPrivateField<EconomyService>(
                bootstrap,
                "_economy");

            Assert.That(economy.Balance, Is.EqualTo(3600d).Within(2d));
        }

        [UnityTest]
        public IEnumerator GameBootstrap_BoostTicksDuringPause()
        {
            var config = CreateConfig(
                production: 1d,
                startsUnlocked: true,
                maxOfflineSeconds: 3600,
                boostDurationSeconds: 10f);
            var bootstrap = CreateBootstrap(
                config,
                new FakeStorageService(),
                new FakeIAPService(),
                new FakeAnalyticsProvider());

            yield return WaitForBootstrapInitialization(bootstrap);

            BoostService boost = GetPrivateField<BoostService>(
                bootstrap,
                "_boost");

            Assert.That(boost.TryActivate(), Is.True);
            float remainingBeforePause = boost.RemainingSeconds;

            InvokePrivateMethod(bootstrap, "OnApplicationPause", true);
            yield return new WaitForSeconds(0.2f);
            InvokePrivateMethod(bootstrap, "OnApplicationPause", false);

            Assert.That(boost.RemainingSeconds, Is.LessThan(remainingBeforePause));
        }

        private GameBootstrapTestAdapter CreateBootstrap(
            GameConfigSO config,
            FakeStorageService storage,
            FakeIAPService iap,
            FakeAnalyticsProvider analytics)
        {
            var bootstrapObject = new GameObject("GameBootstrap");
            _createdObjects.Add(bootstrapObject);

            AddView<FactoryView>("FactoryView");
            AddView<BoostView>("BoostView");
            AddView<BalanceView>("BalanceView");
            AddView<IAPView>("IAPView");

            var bootstrap = bootstrapObject.AddComponent<GameBootstrapTestAdapter>();
            SetPrivateField(bootstrap, "_config", config);
            SetPrivateField(bootstrap, "_factoryView",
                GetLastCreatedObject<FactoryView>());
            SetPrivateField(bootstrap, "_boostView",
                GetLastCreatedObject<BoostView>());
            SetPrivateField(bootstrap, "_balanceView",
                GetLastCreatedObject<BalanceView>());
            SetPrivateField(bootstrap, "_iapView",
                GetLastCreatedObject<IAPView>());

            bootstrap.Configure(analytics, storage, iap);
            return bootstrap;
        }

        private T AddView<T>(string name) where T : Component
        {
            var viewObject = new GameObject(name);
            _createdObjects.Add(viewObject);
            return viewObject.AddComponent<T>();
        }

        private T GetLastCreatedObject<T>() where T : Component
        {
            for (int index = _createdObjects.Count - 1; index >= 0; index--)
            {
                T component = (_createdObjects[index] as GameObject)
                    ?.GetComponent<T>();

                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static IEnumerator WaitForBootstrapInitialization(
            GameBootstrap bootstrap)
        {
            for (int frame = 0; frame < 5; frame++)
            {
                yield return null;
            }

            bool initialized = GetPrivateField<bool>(
                bootstrap,
                "_isInitialized");

            Assert.That(initialized, Is.True);
        }

        private GameConfigSO CreateConfig(
            double production,
            bool startsUnlocked,
            int maxOfflineSeconds,
            float boostDurationSeconds = 10f)
        {
            var machine = new MachineConfig();
            SetPrivateField(machine, "id", "machine_1");
            SetPrivateField(machine, "unlockCost", 100d);
            SetPrivateField(machine, "baseProduction", production);
            SetPrivateField(machine, "baseUpgradeCost", 10d);
            SetPrivateField(machine, "upgradeMultiplier", 1.5d);
            SetPrivateField(machine, "startsUnlocked", startsUnlocked);

            var config = ScriptableObject.CreateInstance<GameConfigSO>();
            _createdObjects.Add(config);
            SetPrivateField(config, "machines", new[] { machine });
            SetPrivateField(config, "boostDurationSeconds", boostDurationSeconds);
            SetPrivateField(config, "boostMultiplier", 2f);
            SetPrivateField(config, "boostEnabled", true);
            SetPrivateField(config, "maxOfflineSeconds", maxOfflineSeconds);
            SetPrivateField(config, "coinsPackAmount", 1000);
            SetPrivateField(config, "coinsPackProductId", "coins_pack_small");
            return config;
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().BaseType?.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            field ??= target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            return (T)field.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            field ??= target.GetType().BaseType?.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            field.SetValue(target, value);
        }

        private static void InvokePrivateMethod(
            object target,
            string methodName,
            params object[] arguments)
        {
            MethodInfo method = target.GetType().BaseType?.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            method.Invoke(target, arguments);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (UnityEngine.Object createdObject in _createdObjects)
            {
                if (createdObject != null)
                {
                    UnityEngine.Object.Destroy(createdObject);
                }
            }

            _createdObjects.Clear();
        }
    }

    internal sealed class GameBootstrapTestAdapter : GameBootstrap
    {
        private IAnalyticsProvider _analyticsProvider;
        private IStorageService _storageService;
        private IIAPService _iapService;

        public void Configure(
            IAnalyticsProvider analyticsProvider,
            IStorageService storageService,
            IIAPService iapService)
        {
            _analyticsProvider = analyticsProvider;
            _storageService = storageService;
            _iapService = iapService;
        }

        protected override IAnalyticsProvider[] CreateAnalyticsProviders()
        {
            return new[] { _analyticsProvider };
        }

        protected override IGameConfig CreateRuntimeConfig(GameConfigSO config)
        {
            return config;
        }

        protected override IStorageService CreateStorageService()
        {
            return _storageService;
        }

        protected override IIAPService CreateIAPService()
        {
            return _iapService;
        }
    }

}
