using Game.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    public class FactoryView : MonoBehaviour
    {
        [SerializeField]
        private MachineView[] _machineViews;

        [SerializeField]
        private TMP_Text _totalProductionText;

        private EconomyService _economy;

        public void Initialize(EconomyService economy, IGameConfig config)
        {
            Unsubscribe();
            _economy = economy;

            if (_economy == null || config == null)
            {
                return;
            }

            int count = Mathf.Min(_machineViews?.Length ?? 0, config.Machines.Count);
            count = Mathf.Min(count, _economy.Machines.Count);

            for (int index = 0; index < count; index++)
            {
                if (_machineViews[index] != null)
                {
                    _machineViews[index].Bind(
                        config.Machines[index],
                        _economy.Machines[index],
                        _economy);
                }
            }

            Subscribe();
            RefreshTotalProduction();
        }

        private void OnEnable()
        {
            Subscribe();
            RefreshTotalProduction();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (isActiveAndEnabled && _economy != null)
            {
                _economy.MachineUnlocked += OnMachineChanged;
                _economy.MachineUpgraded += OnMachineChanged;
            }
        }

        private void Unsubscribe()
        {
            if (_economy != null)
            {
                _economy.MachineUnlocked -= OnMachineChanged;
                _economy.MachineUpgraded -= OnMachineChanged;
            }
        }

        private void OnMachineChanged(MachineModel machine)
        {
            RefreshTotalProduction();
        }

        private void RefreshTotalProduction()
        {
            if (_totalProductionText != null && _economy != null)
            {
                _totalProductionText.text =
                    $"Total: {_economy.TotalProductionPerSecond:F1}/s";
            }
        }
    }
}
