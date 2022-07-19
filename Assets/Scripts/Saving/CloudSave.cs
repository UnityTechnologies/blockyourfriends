using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;
using System.Threading.Tasks;

public class CloudSave : MonoBehaviour
{
    public async void SaveCloudData(string key, object value)
    {
        try
        {
            var data = new Dictionary<string, object> { { key, value } };
            await CloudSaveService.Instance.Data.ForceSaveAsync(data);
            //Debug.Log("Saved data: " + key);
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
    }

    public async Task<T> LoadCloudData<T>(string key, object defaultValue)
    {
        T returnValue = default(T);

        try
        {
            var results = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { key });

            if (results.TryGetValue(key, out string value))
            {
                returnValue = (T)Convert.ChangeType(value, typeof(T));
                //Debug.Log("Loaded data: " + key);
            }
            else
            {
                returnValue = (T)Convert.ChangeType(defaultValue, typeof(T));
                SaveCloudData(key, returnValue);
                //Debug.Log("No key, saved data: " + key);
            }
            return returnValue;
        }

        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
        return returnValue;
    }
}
