using System;
using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using BlockYourFriends.Utility;
using UnityEngine;

public class AppsFlyerManager : Singleton<AppsFlyerManager>, IAppsFlyerConversionData
{
    private const string DevKey = "M64MsPUep3JK3NZo85QrcG";
    private const string AppId = "1611839181";

    void Start()
    {
        /* AppsFlyer.setDebugLog(true); */
    #if UNITY_IOS && !UNITY_EDITOR
        AppsFlyeriOS.waitForATTUserAuthorizationWithTimeoutInterval(60);
    #endif
        AppsFlyer.initSDK(DevKey, AppId, this);
        AppsFlyer.startSDK();
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
        Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
        // add direct deeplink logic here
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("onConversionDataFail", error);
    }

    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
        Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
        // add deferred deeplink logic here
    }

    public void SendAfPurchaseEvent(string currency, string revenue)
    {
        Dictionary<string, string> purchaseEvent = new Dictionary<string, string>();
        purchaseEvent.Add(AFInAppEvents.CURRENCY, currency);
        purchaseEvent.Add(AFInAppEvents.REVENUE, revenue);
        purchaseEvent.Add(AFInAppEvents.QUANTITY, "1");
        purchaseEvent.Add(AFInAppEvents.CONTENT_TYPE, "category_a");
        AppsFlyer.sendEvent("af_purchase", purchaseEvent);
    }
}
