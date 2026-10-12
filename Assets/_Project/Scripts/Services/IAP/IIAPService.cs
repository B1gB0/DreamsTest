using System;
using System.Threading.Tasks;

namespace Game.Services.IAP
{
    public interface IIAPService
    {
        bool IsAvailable { get; }
        event Action<string> OnPurchaseSuccess;
        event Action<string, string> OnPurchaseFailed;
        Task<bool> InitializeAsync();
        Task PurchaseAsync(string productId);
    }
}
