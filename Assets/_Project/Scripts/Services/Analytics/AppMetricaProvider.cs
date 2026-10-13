using System.Text;
using System.Collections.Generic;
using UnityEngine;

#if APPMETRICA_ENABLED
using Io.AppMetrica;
#endif

namespace Game.Services.Analytics
{
    /// <summary>
    /// Analytics provider for AppMetrica.
    /// To enable real SDK integration:
    /// 1. Download AppMetrica Unity SDK from https://appmetrica.yandex.ru/
    /// 2. Import .unitypackage into the project.
    /// 3. Add APPMETRICA_ENABLED to Scripting Define Symbols in Player Settings.
    /// Without the define, this provider logs events via Debug.Log for verification.
    /// </summary>
    public sealed class AppMetricaProvider : IAnalyticsProvider
    {
        public AppMetricaProvider(string apiKey)
        {
#if APPMETRICA_ENABLED
            AppMetrica.Activate(new AppMetricaConfig(apiKey));
            IsInitialized = true;
            Debug.Log($"[AppMetrica] Initialized with API key {apiKey.Substring(0, 4)}...");
#else
            IsInitialized = false;
            Debug.Log($"[AppMetrica] SDK not installed. Falling back to Debug.Log. Would use API key {apiKey.Substring(0, Mathf.Min(4, apiKey.Length))}...");
#endif
        }
        
        public bool IsInitialized { get; private set; }

        public void LogEvent(
            string eventName,
            Dictionary<string, object> parameters)
        {
#if APPMETRICA_ENABLED
            if (parameters == null || parameters.Count == 0)
            {
                AppMetrica.ReportEvent(eventName);
                return;
            }

            AppMetrica.ReportEvent(eventName, BuildEventJson(parameters));
#else
            string paramsText = parameters == null || parameters.Count == 0
                ? "{}"
                : string.Join(", ", parameters);

            Debug.Log($"[AppMetrica/Stub] {eventName} {{{paramsText}}}");
#endif
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
