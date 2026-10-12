using Game.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    public class MachineView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _idText;

        [SerializeField]
        private TMP_Text _levelText;

        [SerializeField]
        private TMP_Text _productionText;

        [SerializeField]
        private TMP_Text _costText;

        [SerializeField]
        private Button _actionButton;

        [SerializeField]
        private TMP_Text _buttonLabel;

        private IMachineConfig _config;
        private MachineModel _model;
        private EconomyService _economy;

        public void Bind(
            IMachineConfig config,
            MachineModel model,
            EconomyService economy)
        {
            Unsubscribe();
            RemoveButtonListener();

            _config = config;
            _model = model;
            _economy = economy;

            Subscribe();
            AddButtonListener();
            Refresh();
        }

        private void OnEnable()
        {
            Subscribe();
            AddButtonListener();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
            RemoveButtonListener();
        }

        private void Subscribe()
        {
            if (!isActiveAndEnabled || _economy == null)
            {
                return;
            }

            _economy.MachineUnlocked += OnMachineChanged;
            _economy.MachineUpgraded += OnMachineChanged;
            _economy.BalanceChanged += OnBalanceChanged;
        }

        private void Unsubscribe()
        {
            if (_economy == null)
            {
                return;
            }

            _economy.MachineUnlocked -= OnMachineChanged;
            _economy.MachineUpgraded -= OnMachineChanged;
            _economy.BalanceChanged -= OnBalanceChanged;
        }

        private void AddButtonListener()
        {
            if (isActiveAndEnabled && _actionButton != null && _economy != null)
            {
                _actionButton.onClick.AddListener(OnActionClicked);
            }
        }

        private void RemoveButtonListener()
        {
            if (_actionButton != null)
            {
                _actionButton.onClick.RemoveListener(OnActionClicked);
            }
        }

        private void OnMachineChanged(MachineModel machine)
        {
            if (_model == machine)
            {
                Refresh();
            }
        }

        private void OnBalanceChanged(double balance)
        {
            Refresh();
        }

        private void OnActionClicked()
        {
            if (_model == null || _economy == null)
            {
                return;
            }

            if (_model.IsUnlocked)
            {
                _economy.TryUpgradeMachine(_model.Id);
            }
            else
            {
                _economy.TryUnlockMachine(_model.Id);
            }

            Refresh();
        }

        private void Refresh()
        {
            if (_model == null)
            {
                return;
            }

            if (_idText != null)
            {
                string state = _model.IsUnlocked ? "Unlocked" : "Locked";
                _idText.text = $"{_model.Id} ({state})";
            }

            if (_levelText != null)
            {
                _levelText.text = $"Level: {_model.Level}";
            }

            if (_productionText != null)
            {
                _productionText.text =
                    $"Production: {_model.ProductionPerSecond:F1}/s";
            }

            double cost = _model.IsUnlocked
                ? _model.UpgradeCost
                : _model.UnlockCost;

            if (_costText != null)
            {
                _costText.text = $"Cost: {cost:F0}";
            }

            if (_buttonLabel != null)
            {
                _buttonLabel.text = _model.IsUnlocked
                    ? $"Upgrade ({_model.UpgradeCost:F0})"
                    : $"Unlock ({_model.UnlockCost:F0})";
            }

            if (_actionButton != null)
            {
                _actionButton.interactable = _economy != null;
            }
        }
    }
}
