using System;
using YTGameSDK;

public class PlayablesAdService : IAdService
{
    readonly YTGameWrapper wrapper;

    public PlayablesAdService(YTGameWrapper wrapper)
    {
        this.wrapper = wrapper;
    }

    public bool RewardedAvailable => true;

    public void ShowInterstitial(Action onDone)
    {
        wrapper.RequestInterstitialAd(shown => onDone());
    }

    public void ShowRewarded(string rewardId, Action<bool> onDone)
    {
        wrapper.RequestRewardedAd(rewardId, earned => onDone(earned));
    }
}
