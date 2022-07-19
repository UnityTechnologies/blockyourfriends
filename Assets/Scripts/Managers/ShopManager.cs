using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockYourFriends.Multiplayer.Auth;
using System.Threading.Tasks;
using BlockYourFriends.Utility;
using BlockYourFriends.Ads;

public enum PlayerItem { BasePaddle, BronzePaddle, SilverPaddle, GoldPaddle }

public class ShopManager : MonoBehaviour
{
    //public bool testRemoveAdsButton;

    private static ShopManager _instance;
    public static ShopManager Instance { get { return _instance; } }

    [SerializeField]
    private Button
        buyBronzePaddleButton,
        buySilverPaddleButton,
        buyGoldPaddleButton,
        removeAdsButton;

    [SerializeField]
    private GameObject
        equipBasePaddleButton,
        basePaddleActiveImage,
        basePaddleInactiveImage,
        equipBronzePaddleButton,
        bronzePaddleActiveImage,
        bronzePaddleInactiveImage,
        equipSilverPaddleButton,
        silverPaddleActiveImage,
        silverPaddleInactiveImage,
        equipGoldPaddleButton,
        goldPaddleActiveImage,
        goldPaddleInactiveImage;

    [SerializeField]
    private TextMeshProUGUI
        playerCoinsText,
        bronzePaddleCoinsPriceText,
        silverPaddleCoinsPriceText,
        goldPaddleCoinsPriceText,
        buyCoinsRealPriceText,
        removeAdsText,
        removeAdsRealPriceText,
        purchaseResultText;

    private PlayerItem activePaddle;

    private bool
        bronzePaddlePurchased,
        silverPaddlePurchased,
        goldPaddlePurchased,
        removeAdsPurchased;

    private int
        bronzePaddleCoinsPrice = 250,
        silverPaddleCoinsPrice = 500,
        goldPaddleCoinsPrice = 750,
        buyCoinsAmount = 500,
        playerCoins;

    private float
        buyCoinsRealPrice,
        removeAdsRealPrice;

    //Analytics
    private float storeOpenTime;
    private string purchasedItem; //item bought
    private string purchaseType; //coins or IAP

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        SubIdentity_Authentication.SignedIn += Init;
    }

    private async void Init()
    {
        if (this != null)
        {
            await GetPlayerCoins();
            SetPlayerCoinsText();

            await GetInventory();

            SetPaddleUI();
            SetRemoveAdsButton();
            //SetVirtualPrices();
            SetRealPrices();
            ClearPurchaseResultText();
        }
    }

    private async Task GetPlayerCoins()
    {
        playerCoins = await EconomyManager.Instance.GetPlayerBalance();
    }

    private async Task SavePlayerCoins()
    {
        await EconomyManager.Instance.SetPlayerBalance(playerCoins);
    }

    private void SetPlayerCoinsText()
    {
        playerCoinsText.text = playerCoins.ToString();
    }

    private async Task GetInventory()
    {
        await EconomyManager.Instance.GetPlayerInventory();
    }

    public PlayerItem ActivePaddle
    {
        get { return activePaddle; }
        set { activePaddle = value; }
    }

    public bool BronzePaddlePurchased
    {
        set { bronzePaddlePurchased = value; }
    }

    public bool SilverPaddlePurchased
    {
        set { silverPaddlePurchased = value; }
    }

    public bool GoldPaddlePurchased
    {
        set { goldPaddlePurchased = value; }
    }

    private void CanBuyPaddles()
    {
        if (playerCoins >= bronzePaddleCoinsPrice)
        {
            buyBronzePaddleButton.interactable = true;
            bronzePaddleCoinsPriceText.color = Color.white;
        }
        else
        {
            buyBronzePaddleButton.interactable = false;
            bronzePaddleCoinsPriceText.color = Color.red;
        }

        if (playerCoins >= silverPaddleCoinsPrice)
        {
            buySilverPaddleButton.interactable = true;
            silverPaddleCoinsPriceText.color = Color.white;
        }
        else
        {
            buySilverPaddleButton.interactable = false;
            silverPaddleCoinsPriceText.color = Color.red;
        }

        if (playerCoins >= goldPaddleCoinsPrice)
        {
            buyGoldPaddleButton.interactable = true;
            goldPaddleCoinsPriceText.color = Color.white;
        }
        else
        {
            buyGoldPaddleButton.interactable = false;
            goldPaddleCoinsPriceText.color = Color.red;
        }
    }

    public void EquipBasePaddle()
    {
        activePaddle = PlayerItem.BasePaddle;
        SetPaddleUI();
        SaveActivePaddle();
    }

    public async void BuyBronzePaddle()
    {
        bronzePaddlePurchased = true;
        playerCoins -= bronzePaddleCoinsPrice;
        SetPlayerCoinsText();
        CanBuyPaddles();
        SetPaddleUI();
        purchasedItem = "bronzePaddle";
        purchaseType = "coins";

        await EconomyManager.Instance.MakeVirtualPurchase(PlayerItem.BronzePaddle);
    }

    public void EquipBronzePaddle()
    {
        activePaddle = PlayerItem.BronzePaddle;
        SetPaddleUI();
        SaveActivePaddle();
    }

    public async void BuySilverPaddle()
    {
        silverPaddlePurchased = true;
        playerCoins -= silverPaddleCoinsPrice;
        SetPlayerCoinsText();
        CanBuyPaddles();
        SetPaddleUI();
        purchasedItem = "silverPaddle";
        purchaseType = "coins";

        await EconomyManager.Instance.MakeVirtualPurchase(PlayerItem.SilverPaddle);
    }

    public void EquipSilverPaddle()
    {
        activePaddle = PlayerItem.SilverPaddle;
        SetPaddleUI();
        SaveActivePaddle();
    }

    public async void BuyGoldPaddle()
    {
        goldPaddlePurchased = true;
        playerCoins -= goldPaddleCoinsPrice;
        SetPlayerCoinsText();
        CanBuyPaddles();
        SetPaddleUI();
        purchasedItem = "goldPaddle";
        purchaseType = "coins";

        await EconomyManager.Instance.MakeVirtualPurchase(PlayerItem.GoldPaddle);
    }

    public void EquipGoldPaddle()
    {
        activePaddle = PlayerItem.GoldPaddle;
        SetPaddleUI();
        SaveActivePaddle();
    }

    private void SetPaddleUI()
    {
        SetBasePaddleUI();
        SetBronzePaddleUI();
        SetSilverPaddleUI();
        SetGoldPaddleUI();
    }

    private async void SaveActivePaddle()
    {
        await EconomyManager.Instance.SaveActivePaddle(activePaddle);
    }

    private void SetBasePaddleUI()
    {
        bool baseActive = activePaddle == PlayerItem.BasePaddle;

        basePaddleActiveImage.SetActive(baseActive);
        basePaddleInactiveImage.SetActive(!baseActive);
        equipBasePaddleButton.SetActive(!baseActive);
    }

    private void SetBronzePaddleUI()
    {
        bool bronzeActive = activePaddle == PlayerItem.BronzePaddle;

        bronzePaddleActiveImage.SetActive(bronzeActive);
        bronzePaddleInactiveImage.SetActive(!bronzeActive);
        buyBronzePaddleButton.gameObject.SetActive(!bronzePaddlePurchased);
        equipBronzePaddleButton.SetActive(bronzePaddlePurchased && !bronzeActive);
    }

    private void SetSilverPaddleUI()
    {
        bool silverActive = activePaddle == PlayerItem.SilverPaddle;

        silverPaddleActiveImage.SetActive(silverActive);
        silverPaddleInactiveImage.SetActive(!silverActive);
        buySilverPaddleButton.gameObject.SetActive(!silverPaddlePurchased);
        equipSilverPaddleButton.SetActive(silverPaddlePurchased && !silverActive);
    }

    private void SetGoldPaddleUI()
    {
        bool goldActive = activePaddle == PlayerItem.GoldPaddle;

        goldPaddleActiveImage.SetActive(goldActive);
        goldPaddleInactiveImage.SetActive(!goldActive);
        buyGoldPaddleButton.gameObject.SetActive(!goldPaddlePurchased);
        equipGoldPaddleButton.SetActive(goldPaddlePurchased && !goldActive);
    }

    //private void SetVirtualPrices()
    //{
    //    //todo inline coin sprites
    //    bronzePaddleCoinsPriceText.text = bronzePaddleCoinsPrice.ToString();
    //    silverPaddleCoinsPriceText.text = silverPaddleCoinsPrice.ToString();
    //    goldPaddleCoinsPriceText.text = goldPaddleCoinsPrice.ToString();
    //}

    private void SetRealPrices()
    {
        buyCoinsRealPriceText.text = IAPManager.BuyCoinsRealPriceString;
        removeAdsRealPriceText.text = IAPManager.BuyRemoveAdsRealPriceString;
    }

    public async void AddCoins(int newCoins)
    {
        playerCoins += newCoins;
        await SavePlayerCoins();
    }

    public void PurchaseCoins()
    {
        //todo real money purchase
    }

    public async void CoinsPurchased()
    {
        playerCoins += buyCoinsAmount;
        SetPlayerCoinsText();
        CanBuyPaddles();
        UpdatePurchaseResultText("Thank you for your purchase!");
        purchasedItem = "coins";
        purchaseType = "IAP";

        await SavePlayerCoins();
    }

    public bool AdsRemoved
    {
        set { removeAdsPurchased = value; }
    }

    public void SetRemoveAdsButton()
    {
        //if (testRemoveAdsButton)
        //    return;

        if (removeAdsPurchased)
            DisableRemoveAdsButton();
    }

    private void DisableRemoveAdsButton()
    {
        removeAdsText.text = "ads removed!";
        removeAdsButton.interactable = false;
        removeAdsRealPriceText.color = Color.grey;
    }

    public void PurchaseRemoveAds()
    {
        //todo real money purchase
    }

    public async void RemoveAdsPurchased()
    {
        removeAdsPurchased = true;
        DisableRemoveAdsButton();
        UpdatePurchaseResultText("Thank you for your purchase!");
        purchasedItem = "removeAds";
        purchaseType = "IAP";

        await EconomyManager.Instance.AddRemoveAdsInventoryItem();
    }

    public void UpdatePurchaseResultText(string text)
    {
        purchaseResultText.text = text;
    }

    private void ClearPurchaseResultText()
    {
        purchaseResultText.text = "";
    }

    public void OpenShop()
    {
        CanBuyPaddles();
        SetPlayerCoinsText();
        AnalyticsShopOpened();
    }

    public void CloseShop()
    {
        AnalyticsShopClosed();
    }

    private void AnalyticsShopOpened()
    {
        purchaseType = "none";
        purchasedItem = "none";
        storeOpenTime = Time.time;
    }

    private void AnalyticsShopClosed()
    {
        float timeClosed = Time.time;
        float timeElapsed = timeClosed - storeOpenTime;
        BYFAnalytics.Instance.ReportStoreActivity(timeElapsed, purchasedItem, purchaseType);
    }

    private void OnDestroy()
    {
        SubIdentity_Authentication.SignedIn -= Init;
    }
}