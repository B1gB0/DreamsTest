using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if APPMETRICA_ENABLED
using Io.AppMetrica;
#endif

namespace Game.Services.Analytics
{
    /// <summary>
    /// Analytics provider for AppMetrica.
    /// If the API key is a placeholder or SDK fails to initialize,
    /// the provider works in degraded mode: events are logged via Debug.Log only.
    /// This prevents the whole game bootstrap from failing if AppMetrica is unavailable.
    /// </summary>
    public sealed class AppMetricaProvider : IAnalyticsProvider
    {
        private const string PlaceholderKeyPrefix = "REPLACE_";

        public AppMetricaProvider(string apiKey)
        {
            IsInitialized = false;

            if (string.IsNullOrEmpty(apiKey) ||
                apiKey.StartsWith(PlaceholderKeyPrefix, StringComparison.Ordinal))
            {
                Debug.LogWarning(
                    "[AppMetrica] API key is a placeholder. SDK not activated. " +
                    "Events will be logged via Debug.Log only. " +
                    "To enable real AppMetrica, replace the key in " +
                    "GameBootstrap.AppMetricaApiKey.");
                return;
            }

            try
            {
#if APPMETRICA_ENABLED
                AppMetrica.Activate(new AppMetricaConfig(apiKey));
                IsInitialized = true;
                Debug.Log($"[AppMetrica] Initialized with API key {apiKey.Substring(0, 4)}...");
#else
                Debug.Log("[AppMetrica] SDK not enabled (APPMETRICA_ENABLED not defined).");
#endif
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[AppMetrica] Failed to initialize: {exception.Message}. " +
                    "Continuing in degraded mode.");
                IsInitialized = false;
            }
        }

        public bool IsInitialized { get; private set; }

        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
#if APPMETRICA_ENABLED
            if (IsInitialized)
            {
                try
                {
                    if (parameters == null || parameters.Count == 0)
                    {
                        AppMetrica.ReportEvent(eventName);
                    }
                    else
                    {
                        AppMetrica.ReportEvent(eventName, BuildEventJson(parameters));
                    }

                    return;
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"[AppMetrica] Failed to log event '{eventName}': " +
                        $"{exception.Message}");
                }
            }
#endif
            string paramsText = parameters == null || parameters.Count == 0
                ? "{}"
                : string.Join(", ", parameters);
            Debug.Log($"[AppMetrica/Degraded] {eventName} {{{paramsText}}}");
        }

#if APPMETRICA_ENABLED
        private static string BuildEventJson(Dictionary<string, object> parameters)
        {
            var builder = new StringBuilder("{");
            bool isFirst = true;

            foreach (var pair in parameters)
            {
                if (!isFirst)
                {
                    builder.Append(',');
                }

                builder.Append('"');
                builder.Append(EscapeJson(pair.Key));
                builder.Append("\":\"");
                builder.Append(EscapeJson(pair.Value?.ToString() ?? string.Empty));
                builder.Append('"');
                isFirst = false;
            }

            builder.Append('}');
            return builder.ToString();
        }

        private static string EscapeJson(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
#endif
    }
}
