using System;
using System.Threading.Tasks;
using Game.Config;
using Game.Domain;
using Game.Presentation;
using Game.Services.Analytics;
using Game.Services.IAP;
using Game.Services.Storage;
using UnityEngine;

namespace Game.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField]
        private GameConfigSO _config;

        [SerializeField]
        private FactoryView _factoryView;

        [SerializeField]
        private BoostView _boostView;

        [SerializeField]
        private BalanceView _balanceView;

        [SerializeField]
        private IAPView _iapView;

        private AnalyticsService _analytics;
        private IStorageService _storage;
        private IIAPService _iap;
        private EconomyService _economy;
        private BoostService _boost;
        private SaveData _saveData;
        private string _lastExitTimeUtc;
        private DateTime _pauseStartedUtc;
        private bool _isInitialized;

        private void Start()
        {
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            if (_config == null)
            {
                Debug.LogError("GameBootstrap requires a GameConfigSO reference.");
                return;
            }

            try
            {
                IAnalyticsProvider[] providers = CreateAnalyticsProviders();
                _analytics = new AnalyticsService(providers);
                _storage = CreateStorageService();

                bool hasSave = _storage.Has("save");
                _saveData = hasSave
                    ? _storage.Load<SaveData>("save")
                    : SaveData.NewGame(_config.Machines.Count);

                if (_saveData == null)
                {
                    _saveData = SaveData.NewGame(_config.Machines.Count);
                    hasSave = false;
                }

                _economy = new EconomyService(_config, _config.Machines);
                if (hasSave)
                {
                    _economy.LoadFromSaveData(_saveData, _config.Machines);
                }

                _boost = new BoostService(
                    _config.BoostDurationSeconds,
                    _config.BoostMultiplier,
                    _config.BoostEnabled);
                
                _boost.BoostStarted += OnBoostStarted;
                _boost.BoostFinished += OnBoostFinished;

                _lastExitTimeUtc = _saveData.LastExitTimeUtc;
                ApplyOfflineIncome(_lastExitTimeUtc);

                _iap = CreateIAPService();
                bool iapInitialized;
                try
                {
                    iapInitialized = await _iap.InitializeAsync();
                }
                catch (Exception exception)
                {
                    iapInitialized = false;
                    Debug.LogException(exception);
                }

                if (!iapInitialized)
                {
                    _analytics.LogEvent("iap_unavailable");
                }

                _iapView?.Initialize(_iap, _config);

                _iap.OnPurchaseSuccess += OnPurchaseSuccess;
                _iap.OnPurchaseFailed += OnPurchaseFailed;

                _factoryView?.Initialize(_economy, _config);
                _boostView?.Initialize(_boost, _config);
                _balanceView?.Initialize(_economy);

                _isInitialized = true;
                _analytics.LogGameStarted();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        protected virtual IAnalyticsProvider[] CreateAnalyticsProviders()
        {
            return new IAnalyticsProvider[]
            {
                new ConsoleAnalyticsProvider(),
                new AppMetricaProvider("test-api-key")
            };
        }

        protected virtual IStorageService CreateStorageService()
        {
            return new JsonFileStorageService();
        }

        protected virtual IIAPService CreateIAPService()
        {
            return new UnityIAPService();
        }

        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            _economy.TryTickProduction(Time.deltaTime * _boost.GetCurrentMultiplier());
            _boost.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (_boost != null)
            {
                _boost.BoostStarted -= OnBoostStarted;
                _boost.BoostFinished -= OnBoostFinished;
            }

            if (_iap != null)
            {
                _iap.OnPurchaseSuccess -= OnPurchaseSuccess;
                _iap.OnPurchaseFailed -= OnPurchaseFailed;
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (!_isInitialized)
            {
                return;
            }

            if (paused)
            {
                _pauseStartedUtc = DateTime.UtcNow;
                Save();
                return;
            }

            if (_pauseStartedUtc != default)
            {
                float elapsed = (float)(DateTime.UtcNow - _pauseStartedUtc).TotalSeconds;
                _boost.Tick(elapsed);
                _pauseStartedUtc = default;
            }

            ApplyOfflineIncome(_lastExitTimeUtc);
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        private void Save()
        {
            if (!_isInitialized || _storage == null || _economy == null)
            {
                return;
            }

            SaveData data = _economy.ToSaveData();
            data.LastExitTimeUtc = DateTime.UtcNow.ToString("o");
            data.LastBoostActivationUtc = _boost.LastActivationUtc;
            _lastExitTimeUtc = data.LastExitTimeUtc;
            _saveData = data;
            _storage.Save("save", data);
        }

        private void ApplyOfflineIncome(string lastExitTimeUtc)
        {
            if (!DateTime.TryParse(
                    lastExitTimeUtc,
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out DateTime lastExitUtc))
            {
                return;
            }

            OfflineIncome income = OfflineProgressCalculator.Calculate(
                lastExitUtc,
                _economy.TotalProductionPerSecond,
                _config.MaxOfflineSeconds);

            if (income.Amount <= 0)
            {
                return;
            }

            _economy.TryAddCoins(income.Amount);
            _analytics.LogOfflineIncome(income.Amount, income.Seconds);
            _lastExitTimeUtc = DateTime.UtcNow.ToString("o");
        }

        private void OnPurchaseSuccess(string productId)
        {
            _economy.TryAddCoins(_config.CoinsPackAmount);
            _analytics.LogPurchaseSucceeded(productId);
        }

        private void OnPurchaseFailed(string productId, string reason)
        {
            _analytics.LogPurchaseFailed(productId, reason);
        }

        private void OnBoostStarted()
        {
            _analytics.LogBoostStarted(
                _config.BoostDurationSeconds,
                _config.BoostMultiplier);
        }

        private void OnBoostFinished()
        {
            _analytics.LogBoostFinished();
        }
    }
}
