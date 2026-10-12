using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public interface IGameConfig
    {
        // Will be implemented in the next stage
    }

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

        public EconomyService(IGameConfig config)
        {
            _balance = 0;
            _machines = new List<MachineModel>();
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
