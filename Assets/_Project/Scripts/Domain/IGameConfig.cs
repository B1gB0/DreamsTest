using System.Collections.Generic;

namespace Game.Domain
{
    public interface IGameConfig
    {
        IReadOnlyList<IMachineConfig> Machines { get; }
        float BoostDurationSeconds { get; }
        float BoostMultiplier { get; }
        bool BoostEnabled { get; }
        int MaxOfflineSeconds { get; }
        int CoinsPackAmount { get; }
        string CoinsPackProductId { get; }
    }
}
