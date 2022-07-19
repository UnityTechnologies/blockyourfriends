using UnityEngine;
//using System.Threading.Tasks;
//using BlockYourFriends.Saving;

public class SaveLoadManager : MonoBehaviour
{
    //    private static SaveLoadManager _instance;
    //    public static SaveLoadManager Instance { get { return _instance; } }

    //    private CloudSave cloudSave;

    //    private void Awake()
    //    {
    //        if (_instance != null && _instance != this)
    //        {
    //            Destroy(gameObject);
    //        }
    //        else
    //        {
    //            _instance = this;
    //        }

    //        cloudSave = GetComponent<CloudSave>();
    //    }

    //    public void SavePurchase(PurchaseType purchase)
    //    {
    //        cloudSave.SaveCloudData(purchase.ToString(), true);
    //    }

    //    public async Task<bool> LoadPurchase(PurchaseType purchase)
    //    {
    //        var purchased = await cloudSave.LoadCloudData<bool>(purchase.ToString(), false) == true;
    //        return purchased;
    //    }

    //    public void SaveActiveItem(Item activeItem, Item item)
    //    {
    //        cloudSave.SaveCloudData(activeItem.ToString(), item.ToString());
    //    }

    //    public async Task<string> LoadActiveItem(Item item, /*Item item,*/ Item defaultItem)
    //    {
    //        var activeItem = await cloudSave.LoadCloudData<string>(item.ToString(), defaultItem.ToString());
    //        string activeItemString = activeItem.Trim('"');
    //        return activeItemString;
    //    }

    //    public void SaveItemValue(Item item, int value)
    //    {
    //        cloudSave.SaveCloudData(item.ToString(), value);
    //    }

    //    public async Task<int> LoadItemValue(Item item, int defaultValue)
    //    {
    //        return await cloudSave.LoadCloudData<int>(item.ToString(), defaultValue);
    //    }
}
