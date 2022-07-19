using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlockYourFriends.Ads;
using Unity.Services.Economy;
using Unity.Services.Economy.Internal.Http;
using Unity.Services.Economy.Model;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    private static EconomyManager _instance;
    public static EconomyManager Instance { get { return _instance; } }

    public Action InventoryChecked;

    //Currency
    private const string currencyID = "COINS";

    //Inventory
    private const string BasePaddleID = "BASE_PADDLE";
    private const string BronzePaddleID = "BRONZE_PADDLE";
    private const string SilverPaddleID = "SILVER_PADDLE";
    private const string GoldPaddleID = "GOLD_PADDLE";
    private const string ActivePaddleID = "ACTIVE_PADDLE";
    private const string RemoveAdsID = "REMOVE_ADS";

    //Virtual purchases
    private const string BronzePaddleVirtualPurchaseID = "BRONZE_PADDLE_VIRTUAL_PURCHASE";
    private const string SilverPaddleVirtualPurchaseID = "SILVER_PADDLE_VIRTUAL_PURCHASE";
    private const string GoldPaddleVirtualPurchaseID = "GOLD_PADDLE_VIRTUAL_PURCHASE";

    //Real money purchases
    private const string BuyCoinsRealMoneyPurchaseID = "BUY_COINS_REAL_MONEY_PURCHASE";
    private const string RemoveAdsRealMoneyPurchaseID = "REMOVE_ADS_REAL_MONEY_PURCHASE";

    private string activePaddleInventoryItemID;

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

    public async Task<int> GetPlayerBalance()
    {
        CurrencyDefinition currencyDefinition = await EconomyService.Instance.Configuration.GetCurrencyAsync(currencyID);
        PlayerBalance playerBalance = await currencyDefinition.GetPlayerBalanceAsync();
        //Debug.Log($"Player balance: {playerBalance.Balance}");
        return (int)playerBalance.Balance;
    }

    public async Task SetPlayerBalance(int newAmount)
    {
        PlayerBalance newPlayerBalance = await EconomyService.Instance.PlayerBalances.SetBalanceAsync(currencyID, newAmount);
        //Debug.Log($"New player balance: {newPlayerBalance.Balance}");
    }

    public async Task GetPlayerInventory()
    {
        GetInventoryResult inventoryResult = await EconomyService.Instance.PlayerInventory.GetInventoryAsync();
        List<PlayersInventoryItem> inventoryItems = inventoryResult.PlayersInventoryItems;
        List<string> itemIDs = new List<string>();

        foreach (PlayersInventoryItem inventoryItem in inventoryItems)
        {
            //Debug.Log($"Owned: {inventoryItem.InventoryItemId}");

            itemIDs.Add(inventoryItem.InventoryItemId);

            switch (inventoryItem.InventoryItemId)
            {
                case BronzePaddleID:
                    ShopManager.Instance.BronzePaddlePurchased = true;
                    break;
                case SilverPaddleID:
                    ShopManager.Instance.SilverPaddlePurchased = true;
                    break;
                case GoldPaddleID:
                    ShopManager.Instance.GoldPaddlePurchased = true;
                    break;
                case ActivePaddleID:
                    LoadActivePaddle(inventoryItem);
                    break;
                case RemoveAdsID:
                    ShopManager.Instance.AdsRemoved = true;
                    AdsManager.Instance.AdsRemoved = true;
                    break;
            }
        }

        if (InventoryChecked != null)
            InventoryChecked();

        if (!itemIDs.Contains(ActivePaddleID))
        {
            Dictionary<string, string> instanceData = new Dictionary<string, string>
            {
                { ActivePaddleID, BasePaddleID }
            };

            AddInventoryItemOptions options = new AddInventoryItemOptions
            {
                InstanceData = instanceData
            };

            PlayersInventoryItem newActivePaddleItem = await EconomyService.Instance.PlayerInventory.AddInventoryItemAsync(ActivePaddleID, options);
            activePaddleInventoryItemID = newActivePaddleItem.PlayersInventoryItemId;

            ShopManager.Instance.ActivePaddle = PlayerItem.BasePaddle;
        }
    }

    public async Task AddRemoveAdsInventoryItem()
    {
        await EconomyService.Instance.PlayerInventory.AddInventoryItemAsync(RemoveAdsID);
    }

    private void LoadActivePaddle(PlayersInventoryItem activePaddleInventoryItem)
    {
        activePaddleInventoryItemID = activePaddleInventoryItem.PlayersInventoryItemId;

        var instanceData = activePaddleInventoryItem.InstanceData.GetAs<Dictionary<string, string>>();
        string activePaddle = instanceData[ActivePaddleID];

        switch (activePaddle)
        {
            case BasePaddleID:
                //Debug.Log($"Active paddle: {BasePaddleID}");
                ShopManager.Instance.ActivePaddle = PlayerItem.BasePaddle;
                break;
            case BronzePaddleID:
                //Debug.Log($"Active paddle: {BronzePaddleID}");
                ShopManager.Instance.ActivePaddle = PlayerItem.BronzePaddle;
                break;
            case SilverPaddleID:
                //Debug.Log($"Active paddle: {SilverPaddleID}");
                ShopManager.Instance.ActivePaddle = PlayerItem.SilverPaddle;
                break;
            case GoldPaddleID:
                //Debug.Log($"Active paddle: {GoldPaddleID}");
                ShopManager.Instance.ActivePaddle = PlayerItem.GoldPaddle;
                break;
        }
    }

    public async Task SaveActivePaddle(PlayerItem activePaddle)
    {
        string newActivePaddle = "";

        switch (activePaddle)
        {
            case PlayerItem.BasePaddle:
                newActivePaddle = BasePaddleID;
                break;
            case PlayerItem.BronzePaddle:
                newActivePaddle = BronzePaddleID;
                break;
            case PlayerItem.SilverPaddle:
                newActivePaddle = SilverPaddleID;
                break;
            case PlayerItem.GoldPaddle:
                newActivePaddle = GoldPaddleID;
                break;
        }

        Dictionary<string, string> instanceData = new Dictionary<string, string>
        {
            { ActivePaddleID, newActivePaddle }
        };

        await EconomyService.Instance.PlayerInventory.UpdatePlayersInventoryItemAsync(activePaddleInventoryItemID, instanceData, null);
    }

    public async Task MakeVirtualPurchase(PlayerItem purchaseItem)
    {
        string purchaseID = "";

        switch (purchaseItem)
        {
            case PlayerItem.BronzePaddle:
                purchaseID = BronzePaddleVirtualPurchaseID;
                break;
            case PlayerItem.SilverPaddle:
                purchaseID = SilverPaddleVirtualPurchaseID;
                break;
            case PlayerItem.GoldPaddle:
                purchaseID = GoldPaddleVirtualPurchaseID;
                break;
        }

        await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync(purchaseID);
    }

    public async Task RedeemAppleAppStorePurchase()
    {
        Debug.Log("RedeemAppleAppStorePurchase");
        RedeemAppleAppStorePurchaseArgs args = new RedeemAppleAppStorePurchaseArgs("PURCHASE_ID", "RECEIPT_FROM_APP_STORE", 0, "USD");
        RedeemAppleAppStorePurchaseResult purchaseResult = await EconomyService.Instance.Purchases.RedeemAppleAppStorePurchaseAsync(args);
        //EconomyAppleAppStorePurchaseFailedException
    }

    public async Task RedeemGooglePlayStorePurchase()
    {
        Debug.Log("RedeemGooglePlayStorePurchase");
        RedeemGooglePlayStorePurchaseArgs args = new RedeemGooglePlayStorePurchaseArgs("PURCHASE_ID", "PURCHASE_DATA", "PURCHASE_DATA_SIGNATURE", 0, "USD");
        RedeemGooglePlayPurchaseResult purchaseResult = await EconomyService.Instance.Purchases.RedeemGooglePlayPurchaseAsync(args);
        //EconomyGooglePlayStorePurchaseFailedException
    }
}