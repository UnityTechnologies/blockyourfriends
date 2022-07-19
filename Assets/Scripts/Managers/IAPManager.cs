using System;
using BlockYourFriends.Ads;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener
{
    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    public static string
        BuyCoinsRealPriceString,
        BuyRemoveAdsRealPriceString;

#if UNITY_ANDROID
    private string buyCoins = "buy_coins";
    private string buyRemoveAds = "buy_remove_ads";
#elif UNITY_IOS
    private string buyCoins = "buyCoins";
    private string buyRemoveAds = "buyRemoveAds";
#endif

    private void Start()
    {
        if (storeController == null)
            InitializePurchasing();
    }

    public void InitializePurchasing()
    {
        if (IsInitialized())
            return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(buyCoins, ProductType.Consumable);
        builder.AddProduct(buyRemoveAds, ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public void GetLocalizedPriceStrings()
    {
        BuyCoinsRealPriceString = storeController.products.WithID(buyCoins).metadata.localizedPriceString;
        BuyRemoveAdsRealPriceString = storeController.products.WithID(buyRemoveAds).metadata.localizedPriceString;
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    public void BuyCoins()
    {
        BuyProductID(buyCoins);
    }

    public void BuyRemoveAds()
    {
        BuyProductID(buyRemoveAds);
    }

    private void BuyProductID(string productId)
    {
        if (!IsInitialized())
        {
            Debug.Log("BuyProductID failed. Not initialized.");
            ShopManager.Instance.UpdatePurchaseResultText("Purchase failed, purchasing not initialized");
            return;
        }

        Product product = storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
        {
            storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.Log("BuyProductID failed. Not purchasing product, either not found or not available for purchase.");
            ShopManager.Instance.UpdatePurchaseResultText("Purchase failed. Not purchasing product, either not found or not available for purchase");
        }
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases failed. Not initialized.");
            ShopManager.Instance.UpdatePurchaseResultText("Restore purchases failed, not initialized");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("RestorePurchases started ...");
            ShopManager.Instance.UpdatePurchaseResultText("Restoring purchases...");

            var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result) =>
            {
                Debug.Log($"RestorePurchases continuing: {result}. If no further messages, no purchases available to restore.");
                ShopManager.Instance.UpdatePurchaseResultText
                ($"Restore purchases continuing: {result}. If no further messages, no purchases available to restore");
            });
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("RestorePurchases started ...");

            var google = storeExtensionProvider.GetExtension<IGooglePlayStoreExtensions>();
            google.RestoreTransactions((result) =>
            {
                Debug.Log($"RestorePurchases continuing: {result}. If no further messages, no purchases available to restore.");
                ShopManager.Instance.UpdatePurchaseResultText
                ($"Restore purchases continuing: {result}. If no further messages, no purchases available to restore");
            });
        }
        else
        {
            Debug.Log($"RestorePurchases failed. Not supported on this platform. Current = {Application.platform}");
            ShopManager.Instance.UpdatePurchaseResultText("Restore purchases failed, not supported on this platform");
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        GetLocalizedPriceStrings();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log($"OnInitializeFailed InitializationFailureReason: {error}");
        ShopManager.Instance.UpdatePurchaseResultText($"Purchasing failed to initialize: {error}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        bool isProductPurchaseFailed = false;

        if (string.Equals(args.purchasedProduct.definition.id, buyCoins, StringComparison.Ordinal))
            ShopManager.Instance.CoinsPurchased();
        else if (string.Equals(args.purchasedProduct.definition.id, buyRemoveAds, StringComparison.Ordinal))
        {
            ShopManager.Instance.RemoveAdsPurchased();
            AdsManager.Instance.AdsRemoved = true;
        }
        else
        {
            isProductPurchaseFailed = true;
            Debug.Log($"ProcessPurchase failed, unrecognized product: '{args.purchasedProduct.definition.id}'");
            ShopManager.Instance.UpdatePurchaseResultText($"Purchase failed, unrecognized product: '{args.purchasedProduct.definition.id}'");
        }

        if(!isProductPurchaseFailed)
            AppsFlyerManager.Instance.SendAfPurchaseEvent(args.purchasedProduct.metadata.isoCurrencyCode, args.purchasedProduct.metadata.localizedPriceString);

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"OnPurchaseFailed: Product: '{product.definition.storeSpecificId}', reason: {failureReason}");
        ShopManager.Instance.UpdatePurchaseResultText($"Purchase failed: Product: '{product.definition.storeSpecificId}', reason: {failureReason}");
    }
}