using System;
using System.Collections;
using Licht.Unity.Objects;
using TMPro;
using Unity.VisualScripting;

public class ShopIndicator : BaseUIObject
{
    public TMP_Text ShopText;
    private ChallengeHandler _challengeHandler;
    private ColorDefaults _colorDefaults;

    protected override void OnAwake()
    {
        base.OnAwake();
        _challengeHandler = SceneObject<ChallengeHandler>.Instance();
        _colorDefaults = SceneObject<ColorDefaults>.Instance();
    }

    private void OnEnable()
    {
        ShopText.text = !_challengeHandler.IsActive ? "" : _challengeHandler.ShopIn == 1
            ? $"Shop in <color=#{_colorDefaults.Healthy.Color.ToHexString()}>NEXT </color>activity"
            : $"Shop in <color=#{_colorDefaults.Recharging.Color.ToHexString()}>{_challengeHandler.ShopIn}</color> activities";

        _challengeHandler.OnChallengeEnd += OnChallengeEvent;
        _challengeHandler.OnChallengeHandlerActivated += OnChallengeEvent;
        _challengeHandler.OnShopStart += SetShopMode;
    }

    private void OnDisable()
    {
        _challengeHandler.OnChallengeEnd -= OnChallengeEvent;
        _challengeHandler.OnChallengeHandlerActivated -= OnChallengeEvent;
        _challengeHandler.OnShopStart -= SetShopMode;
    }

    private void OnChallengeEvent()
    {
        ShopText.text = !_challengeHandler.IsActive ? "" : _challengeHandler.ShopIn == 1
            ? $"Shop in <color=#{_colorDefaults.Healthy.Color.ToHexString()}>NEXT </color>activity"
            : $"Shop in <color=#{_colorDefaults.Recharging.Color.ToHexString()}>{_challengeHandler.ShopIn}</color> activities";
    }

    public void SetShopMode()
    {
        ShopText.text = "Purgatory Shop";
    }
}

