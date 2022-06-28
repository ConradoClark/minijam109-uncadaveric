using System;
using System.Collections.Generic;
using System.Linq;
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
    private LevelManager _levelManager;

    public bool IsActive { get; private set; }

    public int DefaultShopIn;
    public int ShopIn { get; private set; }

    public event Action OnChallengeEnd;
    public event Action OnChallengeHandlerActivated;
    public event Action OnShopStart;
    private BaseChallenge _lastChallenge;

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
        _levelManager = SceneObject<LevelManager>.Instance();
    }

    private void OnEnable()
    {
        DefaultMachinery.AddBasicMachine(HandleChallenges());
    }

    private IEnumerable<IEnumerable<Action>> HandleChallenges()
    {
        ShopIn = DefaultShopIn;
        while (isActiveAndEnabled)
        {
            while (!IsActive)
            {
                yield return TimeYields.WaitOneFrameX;
            }

            OnChallengeHandlerActivated?.Invoke();

            var availableChallenges =
                Challenges.Where(c => _levelManager.Level >= c.LevelMin && _levelManager.Level <= c.LevelMax && c != _lastChallenge);
            var dice = new WeightedDice<BaseChallenge>(availableChallenges, this);
            var challenge = dice.Generate();

            _lastChallenge = challenge;
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
