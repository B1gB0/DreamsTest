using System;
using System.Collections.Generic;
using System.Linq;
using Game.Services.Storage;

namespace Game.Domain
{
    public class EconomyService
    {
        private double _balance;
        private List<MachineModel> _machines;

        public double Balance 
        { 
            get => _balance; 
            private set 
            { 
                _balance = value;
                BalanceChanged?.Invoke(_balance);
            } 
        }

        public List<MachineModel> Machines 
        { 
            get => _machines; 
            private set => _machines = value; 
        }

        public double TotalProductionPerSecond 
        { 
            get 
            {
                return _machines.Where(m => m.IsUnlocked).Sum(m => m.ProductionPerSecond);
            } 
        }

        public event Action<double> BalanceChanged;
        public event Action<MachineModel> MachineUnlocked;
        public event Action<MachineModel> MachineUpgraded;
        public event Action<SaveData> StateChanged;

        public EconomyService(IGameConfig config)
            : this(config, config?.Machines)
        {
        }

        public EconomyService(IGameConfig config, IReadOnlyList<IMachineConfig> configs)
        {
            _balance = 0;
            _machines = new List<MachineModel>();

            if (configs == null)
            {
                return;
            }

            foreach (IMachineConfig machineConfig in configs)
            {
                _machines.Add(new MachineModel(
                    machineConfig,
                    machineConfig.StartsUnlocked,
                    1));
            }
        }

        public SaveData ToSaveData()
        {
            var data = new SaveData
            {
                Balance = Balance,
                MachineLevels = new int[_machines.Count],
                MachineUnlocked = new bool[_machines.Count]
            };

            for (int index = 0; index < _machines.Count; index++)
            {
                data.MachineLevels[index] = _machines[index].Level;
                data.MachineUnlocked[index] = _machines[index].IsUnlocked;
            }

            return data;
        }

        public void LoadFromSaveData(
            SaveData data,
            IReadOnlyList<IMachineConfig> configs)
        {
            if (data == null || configs == null)
            {
                return;
            }

            Balance = data.Balance;
            _machines = new List<MachineModel>(configs.Count);

            for (int index = 0; index < configs.Count; index++)
            {
                int level = data.MachineLevels != null && index < data.MachineLevels.Length
                    ? data.MachineLevels[index]
                    : 1;
                bool isUnlocked = data.MachineUnlocked != null
                    && index < data.MachineUnlocked.Length
                    && data.MachineUnlocked[index];

                _machines.Add(new MachineModel(configs[index], isUnlocked, level));
            }

            StateChanged?.Invoke(ToSaveData());
        }

        public bool TryAddCoins(double amount)
        {
            if (amount <= 0) return false;

            Balance += amount;
            return true;
        }

        public bool TryUnlockMachine(string id)
        {
            var machine = _machines.FirstOrDefault(m => m.Id == id);
            if (machine == null) return false;

            double balanceCopy = Balance;
            bool result = machine.TryUnlock(ref balanceCopy);
            if (result)
            {
                Balance = balanceCopy;
                MachineUnlocked?.Invoke(machine);
            }
            return result;
        }

        public bool TryUpgradeMachine(string id)
        {
            var machine = _machines.FirstOrDefault(m => m.Id == id);
            if (machine == null) return false;

            double balanceCopy = Balance;
            bool result = machine.TryUpgrade(ref balanceCopy);
            if (result)
            {
                Balance = balanceCopy;
                MachineUpgraded?.Invoke(machine);
            }
            return result;
        }

        public bool TryTickProduction(double deltaTime)
        {
            if (deltaTime <= 0) return false;

            double production = TotalProductionPerSecond * deltaTime;
            return TryAddCoins(production);
        }
    }
}
