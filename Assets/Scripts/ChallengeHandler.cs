using System;
using System.Collections.Generic;
using Licht.Impl.Generation;
using Licht.Impl.Orchestration;
using Licht.Interfaces.Generation;
using Licht.Interfaces.Update;
using Licht.Unity.Objects;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChallengeHandler : BaseGameObject, IGenerator<int,float>, IActivable
{
    public BaseChallenge[] Challenges;

    private Heart _heart;
    private TextBox _textbox;
    private ChallengeComponents _challengeComponents;
    private ColorDefaults _colorDefaults;
    private Shop _shop;
    public bool IsActive { get; private set; }

    public int DefaultShopIn;
    public int ShopIn { get; private set; }

    public event Action OnChallengeEnd;
    public event Action OnChallengeHandlerActivated;
    public event Action OnShopStart;

    public bool Activate()
    {
        if (IsActive) return false;

        IsActive = true;
        return true;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        _heart = SceneObject<Heart>.Instance(true);
        _textbox = SceneObject<TextBox>.Instance();
        _challengeComponents = SceneObject<ChallengeComponents>.Instance();
        _colorDefaults = SceneObject<ColorDefaults>.Instance();
        _shop = SceneObject<Shop>.Instance();
    }

    private void OnEnable()
    {
        DefaultMachinery.AddBasicMachine(HandleChallenges());
    }

    private IEnumerable<IEnumerable<Action>> HandleChallenges()
    {
        var dice = new WeightedDice<BaseChallenge>(Challenges, this);
        ShopIn = DefaultShopIn;
        while (isActiveAndEnabled)
        {
            while (!IsActive)
            {
                yield return TimeYields.WaitOneFrameX;
            }

            OnChallengeHandlerActivated?.Invoke();

            var challenge = dice.Generate();
            yield return challenge.HandleChallenge().AsCoroutine();

            ShopIn -= 1;
            OnChallengeEnd?.Invoke();

            if (ShopIn == 0)
            {
                OnShopStart?.Invoke();
                _challengeComponents.SetActivity("Shop", _colorDefaults.Healthy.Color);
                yield return StartShop().AsCoroutine();
                ShopIn = DefaultShopIn;
            }

            yield return TimeYields.WaitOneFrameX;
        }
    }

    private IEnumerable<IEnumerable<Action>> StartShop()
    {
        yield return _textbox.ShowText("", false).AsCoroutine();

        yield return _shop.SpawnShop().AsCoroutine();
    }

    public int Seed { get; set; }
    public float Generate()
    {
        return Random.value;
    }
}
