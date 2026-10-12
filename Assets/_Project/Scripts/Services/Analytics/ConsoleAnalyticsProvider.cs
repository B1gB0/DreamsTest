using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Services.Analytics
{
    public class ConsoleAnalyticsProvider : IAnalyticsProvider
    {
        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            string values = parameters == null
                ? string.Empty
                : string.Join(", ", parameters.Select(pair => $"{pair.Key}={pair.Value}"));

            Debug.Log($"[Analytics] {eventName} {{{values}}}");
        }
    }
}
