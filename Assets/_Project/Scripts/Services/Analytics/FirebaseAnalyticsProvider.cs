using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_ANALYTICS_ENABLED
using Firebase.Analytics;
#endif

namespace Game.Services.Analytics
{
    /// <summary>
    /// Analytics provider for Firebase Analytics.
    /// Without the define, this provider logs via Debug.Log for verification.
    /// </summary>
    public sealed class FirebaseAnalyticsProvider : IAnalyticsProvider
    {
        public FirebaseAnalyticsProvider()
        {
            try
            {
#if FIREBASE_ANALYTICS_ENABLED
                Debug.Log("[FirebaseAnalytics] Initialized.");
#else
                Debug.Log("[FirebaseAnalytics] SDK not enabled. Falling back to Debug.Log.");
#endif
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning(
                    $"[FirebaseAnalytics] Failed to initialize: {exception.Message}. " +
                    "Continuing in degraded mode.");
            }
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
#if FIREBASE_ANALYTICS_ENABLED
            try
            {
                if (parameters == null || parameters.Count == 0)
                {
                    FirebaseAnalytics.LogEvent(eventName);
                    return;
                }

                var firebaseParams = new List<Parameter>();
                foreach (var pair in parameters)
                {
                    if (pair.Value is string stringValue)
                    {
                        firebaseParams.Add(new Parameter(pair.Key, stringValue));
                    }
                    else if (pair.Value is int intValue)
                    {
                        firebaseParams.Add(new Parameter(pair.Key, intValue));
                    }
                    else if (pair.Value is long longValue)
                    {
                        firebaseParams.Add(new Parameter(pair.Key, longValue));
                    }
                    else if (pair.Value is double doubleValue)
                    {
                        firebaseParams.Add(new Parameter(pair.Key, doubleValue));
                    }
                    else if (pair.Value is float floatValue)
                    {
                        firebaseParams.Add(new Parameter(pair.Key, floatValue));
                    }
                    else if (pair.Value is bool boolValue)
                    {
                        firebaseParams.Add(new Parameter(pair.Key, boolValue ? 1 : 0));
                    }
                    else
                    {
                        firebaseParams.Add(
                            new Parameter(pair.Key, pair.Value?.ToString() ?? string.Empty));
                    }
                }

                FirebaseAnalytics.LogEvent(eventName, firebaseParams.ToArray());
                return;
            }
            catch (System.Exception exception)
            {
                Debug.LogError(
                    $"[FirebaseAnalytics] Failed to log event '{eventName}': " +
                    $"{exception.Message}. Continuing in degraded mode.");
            }
#endif
            string paramsText = parameters == null || parameters.Count == 0
                ? "{}"
                : string.Join(", ", parameters);
            Debug.Log($"[FirebaseAnalytics/Degraded] {eventName} {{{paramsText}}}");
        }
    }
}
