using System.Collections.Generic;

namespace Game.Services.Analytics
{
    public interface IAnalyticsProvider
    {
        void LogEvent(string eventName, Dictionary<string, object> parameters);
    }
}
