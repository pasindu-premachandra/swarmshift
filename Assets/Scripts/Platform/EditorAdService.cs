using System;
using UnityEngine;

public class EditorAdService : IAdService
{
    public bool RewardedAvailable => true;

    public void ShowInterstitial(Action onDone)
    {
        Debug.Log("[Platform] Editor: interstitial requested, none shown");
        onDone();
    }

    public void ShowRewarded(string rewardId, Action<bool> onDone)
    {
        Debug.Log($"[Platform] Editor: rewarded ad {rewardId} requested, none shown, reward granted");
        onDone(true);
    }
}
