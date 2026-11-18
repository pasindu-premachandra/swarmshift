using System;

public interface IAdService
{
    bool RewardedAvailable { get; }

    void ShowInterstitial(Action onDone);
    void ShowRewarded(string rewardId, Action<bool> onDone);
}
