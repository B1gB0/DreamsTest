using Game.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    public class BalanceView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _balanceText;

        private EconomyService _economy;

        public void Initialize(EconomyService economy)
        {
            Unsubscribe();
            _economy = economy;
            Subscribe();
            Refresh();
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (isActiveAndEnabled && _economy != null)
            {
                _economy.BalanceChanged += OnBalanceChanged;
            }
        }

        private void Unsubscribe()
        {
            if (_economy != null)
            {
                _economy.BalanceChanged -= OnBalanceChanged;
            }
        }

        private void OnBalanceChanged(double value)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_balanceText != null && _economy != null)
            {
                _balanceText.text = $"Balance: {_economy.Balance:F0}";
            }
        }
    }
}
