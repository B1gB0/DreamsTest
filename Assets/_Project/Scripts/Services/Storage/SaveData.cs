using System;

namespace Game.Services.Storage
{
    [Serializable]
    public class SaveData
    {
        public double Balance;
        public int[] MachineLevels;
        public bool[] MachineUnlocked;
        public string LastExitTimeUtc;
        public string LastBoostActivationUtc;

        public static SaveData NewGame(int machineCount)
        {
            return new SaveData
            {
                Balance = 0,
                MachineLevels = new int[machineCount],
                MachineUnlocked = new bool[machineCount],
                LastExitTimeUtc = DateTime.UtcNow.ToString("o"),
                LastBoostActivationUtc = string.Empty
            };
        }
    }
}
