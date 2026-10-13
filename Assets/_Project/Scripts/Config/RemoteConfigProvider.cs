using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Domain;
using UnityEngine;

#if FIREBASE_REMOTE_CONFIG_ENABLED
using Firebase.RemoteConfig;
using Firebase.Extensions;
#endif

namespace Game.Config
{
    /// <summary>
    /// Remote Config provider with local fallback.
    /// </summary>
    public sealed class RemoteConfigProvider : IGameConfig
    {
        private const string BoostDurationKey = "boost_duration_seconds";
        private const string BoostMultiplierKey = "boost_multiplier";
        private const string BoostEnabledKey = "boost_enabled";
        private const string MaxOfflineKey = "max_offline_seconds";
        private const string CoinsPackAmountKey = "coins_pack_amount";
        private const string CoinsPackProductIdKey = "coins_pack_product_id";

        private readonly IGameConfig _fallback;
        private readonly Dictionary<string, object> _remoteValues = new();

        public RemoteConfigProvider(IGameConfig fallback)
        {
            _fallback = fallback;
        }

        public IReadOnlyList<IMachineConfig> Machines => _fallback.Machines;

        public float BoostDurationSeconds =>
            GetFloat(BoostDurationKey, _fallback.BoostDurationSeconds);

        public float BoostMultiplier =>
            GetFloat(BoostMultiplierKey, _fallback.BoostMultiplier);

        public bool BoostEnabled =>
            GetBool(BoostEnabledKey, _fallback.BoostEnabled);

        public int MaxOfflineSeconds =>
            GetInt(MaxOfflineKey, _fallback.MaxOfflineSeconds);

        public int CoinsPackAmount =>
            GetInt(CoinsPackAmountKey, _fallback.CoinsPackAmount);

        public string CoinsPackProductId =>
            GetString(CoinsPackProductIdKey, _fallback.CoinsPackProductId);

        public async Task FetchAsync()
        {
#if FIREBASE_REMOTE_CONFIG_ENABLED
            try
            {
                var defaults = new Dictionary<string, object>
                {
                    { BoostDurationKey, _fallback.BoostDurationSeconds },
                    { BoostMultiplierKey, _fallback.BoostMultiplier },
                    { BoostEnabledKey, _fallback.BoostEnabled },
                    { MaxOfflineKey, _fallback.MaxOfflineSeconds },
                    { CoinsPackAmountKey, _fallback.CoinsPackAmount },
                    { CoinsPackProductIdKey, _fallback.CoinsPackProductId }
                };

                await FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);
                await FirebaseRemoteConfig.DefaultInstance.FetchAsync(System.TimeSpan.Zero);
                await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();

                var config = FirebaseRemoteConfig.DefaultInstance;
                _remoteValues[BoostDurationKey] =
                    (float)config.GetValue(BoostDurationKey).DoubleValue;
                _remoteValues[BoostMultiplierKey] =
                    (float)config.GetValue(BoostMultiplierKey).DoubleValue;
                _remoteValues[BoostEnabledKey] =
                    config.GetValue(BoostEnabledKey).BooleanValue;
                _remoteValues[MaxOfflineKey] =
                    (int)config.GetValue(MaxOfflineKey).LongValue;
                _remoteValues[CoinsPackAmountKey] =
                    (int)config.GetValue(CoinsPackAmountKey).LongValue;
                _remoteValues[CoinsPackProductIdKey] =
                    config.GetValue(CoinsPackProductIdKey).StringValue;

                Debug.Log(
                    $"[RemoteConfig] Firebase fetch completed. " +
                    $"Status: {config.Info.LastFetchStatus}");
            }
            catch (System.Exception exception)
            {
                _remoteValues.Clear();
                Debug.LogWarning(
                    $"[RemoteConfig] Firebase initialization/fetch failed: " +
                    $"{exception.Message}. Using local GameConfigSO fallback.");
            }
#else
            Debug.Log("[RemoteConfig] Firebase SDK not enabled. Using local GameConfigSO fallback.");
            await Task.CompletedTask;
#endif
        }

        private float GetFloat(string key, float fallbackValue)
        {
            if (_remoteValues.TryGetValue(key, out var value) && value is float floatValue)
            {
                return floatValue;
            }

            return fallbackValue;
        }

        private int GetInt(string key, int fallbackValue)
        {
            if (_remoteValues.TryGetValue(key, out var value) && value is int intValue)
            {
                return intValue;
            }

            return fallbackValue;
        }

        private bool GetBool(string key, bool fallbackValue)
        {
            if (_remoteValues.TryGetValue(key, out var value) && value is bool boolValue)
            {
                return boolValue;
            }

            return fallbackValue;
        }

        private string GetString(string key, string fallbackValue)
        {
            if (_remoteValues.TryGetValue(key, out var value) && value is string stringValue)
            {
                return stringValue;
            }

            return fallbackValue;
        }
    }
}
