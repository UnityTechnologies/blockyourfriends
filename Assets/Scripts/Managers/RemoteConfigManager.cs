using UnityEngine;
using Unity.Services.RemoteConfig;
using BlockYourFriends.Multiplayer.Auth;

public class RemoteConfigManager : MonoBehaviour
{
    public struct userAttributes { }
    public struct appAttributes { }

    void Awake()
    {
        SubIdentity_Authentication.SignedIn += Init;
    }

    void Init()
    {
        RemoteConfigService.Instance.FetchCompleted += SetBackground;
        RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes());
    }

    void SetBackground(ConfigResponse response)
    {
        var bg = RemoteConfigService.Instance.appConfig.GetString("Background");
        UIManager.Instance.SetBackground(bg);
    }

    void OnDestroy()
    {
        RemoteConfigService.Instance.FetchCompleted -= SetBackground;
    }
}
