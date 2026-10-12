using Game.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    public class BoostView : MonoBehaviour
    {
        [SerializeField]
        private Button _activateButton;

        [SerializeField]
        private TMP_Text _statusText;

        private BoostService _boost;
        private float _multiplier;

        public void Initialize(BoostService boost, IGameConfig config)
        {
            Unsubscribe();
            _boost = boost;
            _multiplier = config != null ? config.BoostMultiplier : 1f;
            Subscribe();
            RefreshStatus();
        }

        private void OnEnable()
        {
            Subscribe();
            RefreshStatus();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (_boost != null && _boost.IsActive)
            {
                RefreshStatus();
            }
        }

        private void Subscribe()
        {
            if (!isActiveAndEnabled || _boost == null)
            {
                return;
            }

            _boost.BoostStarted += OnBoostChanged;
            _boost.BoostFinished += OnBoostChanged;

            if (_activateButton != null)
            {
                _activateButton.onClick.AddListener(OnActivateClicked);
            }
        }

        private void Unsubscribe()
        {
            if (_boost != null)
            {
                _boost.BoostStarted -= OnBoostChanged;
                _boost.BoostFinished -= OnBoostChanged;
            }

            if (_activateButton != null)
            {
                _activateButton.onClick.RemoveListener(OnActivateClicked);
            }
        }

        private void OnBoostChanged()
        {
            RefreshStatus();
        }

        private void OnActivateClicked()
        {
            if (_boost != null)
            {
                _boost.TryActivate();
                RefreshStatus();
            }
        }

        private void RefreshStatus()
        {
            if (_statusText == null || _boost == null)
            {
                return;
            }

            _statusText.text = _boost.IsActive
                ? $"Boost: active ({_boost.RemainingSeconds:F1}s, x{_multiplier:F1})"
                : "Boost: inactive";
        }
    }
}
