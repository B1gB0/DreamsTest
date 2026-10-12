using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Services.IAP
{
    public class UnityIAPService : IIAPService
    {
        public bool IsAvailable { get; private set; }

        public event Action<string> OnPurchaseSuccess;
        public event Action<string, string> OnPurchaseFailed;

        public Task<bool> InitializeAsync()
        {
            IsAvailable = true;
            return Task.FromResult(true);
        }

        public async Task PurchaseAsync(string productId)
        {
            Debug.Log($"[IAP] Purchase requested: {productId}");

            if (!IsAvailable)
            {
                OnPurchaseFailed?.Invoke(productId, "IAP is unavailable.");
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
            OnPurchaseSuccess?.Invoke(productId);
        }
    }
}
