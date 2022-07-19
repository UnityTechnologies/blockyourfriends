using System.Collections;
using UnityEngine;
using System;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
using UnityEngine.iOS;
#endif

public class ATTChecker : MonoBehaviour
{
#if UNITY_IOS
    private void Awake()
    {
        CheckTrackingStatus();
    }

    public void CheckTrackingStatus()
    {
        Version currentVersion = new Version(Device.systemVersion);
        Version ios14 = new Version("14.5");

        if (GetTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED && currentVersion >= ios14)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
            StartCoroutine(CheckStatus());
        }
    }

    private IEnumerator CheckStatus()
    {
        while (GetTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            yield return new WaitForSeconds(1f);
    }

    private ATTrackingStatusBinding.AuthorizationTrackingStatus GetTrackingStatus()
    {
        var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        return status;
    }
#endif
}