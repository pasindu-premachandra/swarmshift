using System;

// For builds with no ad network: Meta (rewarded video needs an approved partner account) and the web build.
public class NoAdService : IAdService
{
    public bool RewardedAvailable => false;

    public void ShowInterstitial(Action onDone)
    {
        onDone();
    }

    public void ShowRewarded(string rewardId, Action<bool> onDone)
    {
        onDone(false);
    }
}
