namespace Foundation.IAP
{
    public delegate void CallBack(PurchaseResult result);
    public interface IPurchaser
    {
        event CallBack OnPurchaseFail;
        event CallBack OnPurchaseSucceed;

        void InitializePurchasing(ProductInfo[] catalog);
        void BuyProductId(string productId);
        void RestorePurchases();
    }
}
