using UnityEngine;
using Foundation.ServicesResolver;
using Foundation.ConfigurationResolver;
using UnityEngine.Purchasing;
using System;
using System.Text;
using Foundation.Logger;
using Result = Foundation.Utils.OperationUtils.Result;

namespace Foundation.IAP
{
    [Serializable]
    public struct ProductInfo
    {
        public string ProductKey;
        public bool IsConsumable;

        public ProductInfo(string prodKey, bool isConsumable)
        {
            ProductKey = prodKey;
            IsConsumable = isConsumable;
        }
    }

    public class PurchaseResult : Result
    {
        public Product Product;
        public PurchaseFailureReason FailureReason;
        public PurchaseEventArgs PurchaseEventArgs;


        public PurchaseResult(Product product, PurchaseFailureReason failureReason, string message = "")
        {
            Product = product;
            FailureReason = failureReason;
            PurchaseEventArgs = null;
            SetFailure(message);
        }

        public PurchaseResult(PurchaseEventArgs purchaseEventArgs)
        {
            Product = null;
            FailureReason = PurchaseFailureReason.Unknown;
            PurchaseEventArgs = purchaseEventArgs;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Purchase successful: " + Success);
            if (!Success)
            {
                sb.AppendLine(FailureReason.ToString());
            }
            if (Messages != null)
            {
                for (int i = 0; i < Messages.Count; i++)
                {
                    sb.AppendLine(i + ": " + Messages[i]);
                }
            }
            return sb.ToString();
        }
    }

    public class IAPService : BaseService, IStoreListener, IPurchaser
    {
        public event CallBack OnPurchaseFail, OnPurchaseSucceed;
        private static IStoreController _storeController;
        private static IExtensionProvider _storeExtensionProvider;
        private ProductInfo[] _catalog;

        private bool IsInitialized => _storeController != null && _storeExtensionProvider != null;

        #region API

        public void InitializePurchasing(ProductInfo[] catalog)
        {
            if (IsInitialized)
            {
                this.LogWarning("IAP Service is already initialized.");
                return;
            }

            _catalog = catalog;
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (ProductInfo p in catalog)
            {
                builder.AddProduct(p.ProductKey, p.IsConsumable ? ProductType.Consumable : ProductType.NonConsumable);
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public void BuyProductId(string productId)
        {
            if (!IsInitialized)
            {
                OnPurchaseFail?.Invoke(new PurchaseResult(null, PurchaseFailureReason.Unknown,
                    "Purchase service is not initialised!"));
                return;
            }

            Product product = _storeController.products.WithID(productId);
            if (product == null)
            {
                OnPurchaseFail?.Invoke(new PurchaseResult(null, PurchaseFailureReason.Unknown,
                    "Product is not found. ID:" + productId));
                return;
            }

            if (!product.availableToPurchase)
            {
                OnPurchaseFail?.Invoke(new PurchaseResult(product, PurchaseFailureReason.ProductUnavailable));
                return;
            }

            this.Log("Purchasing product asynchronously:" + product.definition.id);
            _storeController.InitiatePurchase(product);
        }

        public void RestorePurchases()
        {
            if (!IsInitialized)
            {
                this.LogError("RestorePurchases FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                var apple = _storeExtensionProvider.GetExtension<IAppleExtensions>();
                apple.RestoreTransactions((result) =>
                {
                    this.Log("RestorePurchases continuing: " + result +
                             ". If no further messages, no purchases available to restore.");
                });
            }
            else
            {
                this.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        #endregion

        #region Store interface methods

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _storeExtensionProvider = extensions;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            this.LogError("Initialize Failed! Reason:" + error);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchaseFail?.Invoke(new PurchaseResult(product, failureReason));
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            bool foundProduct = false;
            foreach (var product in _catalog)
            {
                if (string.Equals(args.purchasedProduct.definition.id, product.ProductKey, StringComparison.Ordinal))
                {
                    foundProduct = true;
                    OnPurchaseSucceed?.Invoke(new PurchaseResult(args));
                }
            }
            if (!foundProduct)
            {
                var result = new PurchaseResult(null, PurchaseFailureReason.ProductUnavailable,
                    "Product processing result Fail - can't find " + args.purchasedProduct.definition.id + " in Products Catalog");
                OnPurchaseFail?.Invoke(result);
            }

            //CHANGE PurchaseProcessingResult.Complete to Pending to consume on server side
            return PurchaseProcessingResult.Complete;
        }

        #endregion

        #region Base service methods

        protected override void Initialize()
        {
        }

        public override void Dispose()
        {
            _storeController = null;
            _storeExtensionProvider = null;
            _catalog = null;
            OnPurchaseFail = null;
            OnPurchaseSucceed = null;
        }

        #endregion
    }
}
