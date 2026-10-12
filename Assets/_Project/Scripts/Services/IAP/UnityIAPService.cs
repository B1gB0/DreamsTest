using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Game.Services.IAP
{
    public class UnityIAPService : IIAPService
    {
        private const string CoinsPackProductId = "coins_pack_small";
        private const int InitializationTimeoutMilliseconds = 10000;

        private StoreController _storeController;
        private TaskCompletionSource<bool> _initTcs;

        public bool IsAvailable { get; private set; }

        public event Action<string> OnPurchaseSuccess;
        public event Action<string, string> OnPurchaseFailed;

        public async Task<bool> InitializeAsync()
        {
            if (IsAvailable)
            {
                return true;
            }

            IsAvailable = false;
            _initTcs = new TaskCompletionSource<bool>();

            try
            {
                await UnityServices.InitializeAsync();

                _storeController = UnityIAPServices.StoreController();
                _storeController.OnStoreConnected += OnStoreConnected;
                _storeController.OnStoreDisconnected += OnStoreDisconnected;
                _storeController.OnPurchasePending += OnPurchasePending;
                _storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
                _storeController.OnPurchaseDeferred += OnPurchaseDeferred;
                _storeController.OnPurchaseFailed += OnPurchaseFailedInternal;
                _storeController.OnProductsFetched += OnProductsFetched;
                _storeController.OnProductsFetchFailed += OnProductsFetchFailed;

                await _storeController.Connect();

                Task completedTask = await Task.WhenAny(
                    _initTcs.Task,
                    Task.Delay(InitializationTimeoutMilliseconds));

                if (completedTask != _initTcs.Task)
                {
                    IsAvailable = false;
                    Debug.LogError("Unity IAP initialization timed out.");
                    return false;
                }

                return await _initTcs.Task;
            }
            catch (Exception exception)
            {
                IsAvailable = false;
                _initTcs.TrySetResult(false);
                Debug.LogError($"Unity IAP initialization failed: {exception}");
                return false;
            }
        }

        public async Task PurchaseAsync(string productId)
        {
            if (!IsAvailable)
            {
                OnPurchaseFailed?.Invoke(productId, "IAP unavailable");
                return;
            }

            Product product = _storeController.GetProductByCatalogListingId(productId);
            if (product == null)
            {
                OnPurchaseFailed?.Invoke(productId, "Product not found");
                return;
            }

            _storeController.PurchaseProduct(product);
            await Task.CompletedTask;
        }

        private void OnStoreConnected()
        {
            IsAvailable = true;
            _initTcs.TrySetResult(true);

            var products = new List<ProductDefinition>
            {
                new ProductDefinition(CoinsPackProductId, ProductType.Consumable)
            };

            _storeController.FetchProducts(products);
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription failure)
        {
            IsAvailable = false;
            _initTcs?.TrySetResult(false);
            Debug.LogError($"Store disconnected: {failure}");
        }

        private void OnPurchasePending(PendingOrder order)
        {
            string productId = order.CartOrdered.Items()
                .FirstOrDefault()?.Product?.definition.id;

            if (string.IsNullOrEmpty(productId))
            {
                Debug.LogError("Unity IAP pending order does not contain a product.");
                return;
            }

            _storeController.ConfirmPurchase(order);
        }

        private void OnPurchaseConfirmed(Order order)
        {
            if (!(order is ConfirmedOrder confirmedOrder))
            {
                return;
            }

            string productId = confirmedOrder.CartOrdered.Items()
                .FirstOrDefault()?.Product?.definition.id;

            if (!string.IsNullOrEmpty(productId))
            {
                OnPurchaseSuccess?.Invoke(productId);
            }
        }

        private void OnPurchaseDeferred(DeferredOrder order)
        {
            string productId = order.CartOrdered.Items()
                .FirstOrDefault()?.Product?.definition.id;

            Debug.LogWarning($"Purchase deferred: {productId}");
        }

        private void OnPurchaseFailedInternal(FailedOrder failedOrder)
        {
            string productId = failedOrder.CartOrdered.Items()
                .FirstOrDefault()?.Product?.definition.id ?? string.Empty;
            string reason = failedOrder.FailureReason.ToString();

            OnPurchaseFailed?.Invoke(productId, reason);
        }

        private void OnProductsFetched(List<Product> products)
        {
            Debug.Log($"Unity IAP products fetched: {products.Count}.");
        }

        private void OnProductsFetchFailed(ProductFetchFailed failure)
        {
            Debug.LogError(
                $"Unity IAP products fetch failed: {failure.FailureReason}");
        }
    }
}
