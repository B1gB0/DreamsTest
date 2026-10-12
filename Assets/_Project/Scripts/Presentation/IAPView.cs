using System.Collections;
using Game.Domain;
using Game.Services.IAP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    public class IAPView : MonoBehaviour
    {
        [SerializeField]
        private Button _purchaseButton;

        [SerializeField]
        private TMP_Text _purchaseLabel;

        private IIAPService _iap;
        private IGameConfig _config;
        private Coroutine _feedbackCoroutine;

        public void Initialize(IIAPService iap, IGameConfig config)
        {
            Unsubscribe();
            RemoveButtonListener();

            _iap = iap;
            _config = config;

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

            if (_feedbackCoroutine != null)
            {
                StopCoroutine(_feedbackCoroutine);
                _feedbackCoroutine = null;
            }
        }

        private void Subscribe()
        {
            if (!isActiveAndEnabled || _iap == null)
            {
                return;
            }

            _iap.OnPurchaseSuccess += OnPurchaseSuccess;
            _iap.OnPurchaseFailed += OnPurchaseFailed;
        }

        private void Unsubscribe()
        {
            if (_iap == null)
            {
                return;
            }

            _iap.OnPurchaseSuccess -= OnPurchaseSuccess;
            _iap.OnPurchaseFailed -= OnPurchaseFailed;
        }

        private void AddButtonListener()
        {
            if (isActiveAndEnabled && _purchaseButton != null && _iap != null)
            {
                _purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }
        }

        private void RemoveButtonListener()
        {
            if (_purchaseButton != null)
            {
                _purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
            }
        }

        private void OnPurchaseClicked()
        {
            if (_iap == null || _config == null || !_iap.IsAvailable)
            {
                return;
            }

            _ = _iap.PurchaseAsync(_config.CoinsPackProductId);
        }

        private void OnPurchaseSuccess(string productId)
        {
            ShowFeedback("Purchase complete!");
        }

        private void OnPurchaseFailed(string productId, string reason)
        {
            ShowFeedback($"Purchase failed: {reason}");
        }

        private void Refresh()
        {
            if (_config != null && _purchaseLabel != null)
            {
                _purchaseLabel.text = $"Buy {_config.CoinsPackAmount} coins";
            }

            if (_purchaseButton != null)
            {
                _purchaseButton.interactable =
                    _iap != null && _iap.IsAvailable;
            }
        }

        private void ShowFeedback(string message)
        {
            if (_feedbackCoroutine != null)
            {
                StopCoroutine(_feedbackCoroutine);
            }

            _feedbackCoroutine = StartCoroutine(ShowFeedbackCoroutine(message));
        }

        private IEnumerator ShowFeedbackCoroutine(string message)
        {
            if (_purchaseLabel != null)
            {
                _purchaseLabel.text = message;
            }

            yield return new WaitForSeconds(2f);

            _feedbackCoroutine = null;
            Refresh();
        }
    }
}
