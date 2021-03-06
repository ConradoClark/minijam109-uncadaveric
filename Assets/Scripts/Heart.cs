using System;
using System.Collections.Generic;
using System.Linq;
using Licht.Impl.Memory;
using Licht.Impl.Orchestration;
using Licht.Unity.Objects;
using Licht.Unity.Pooling;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Heart : BaseGameObject
{
    public Transform HeartLaserHelp;
    public Transform EndScreen;
    public Transform SkipTutorialHelp;

    public ScriptInput HeartBeatInput;
    public ScriptInput SkipTutorialInput;

    public int Bpm { get; private set; }
    public int TargetBpm { get; private set; }
    public bool Flatlined { get; private set; }
    public bool Deceased { get; private set; }

    public bool IsDefibrillating { get; private set; }

    public Animator Animator;
    public TMP_Text TargetBpmText;
    public TMP_Text BpmText;
    public ItemCounter Defibrillator;
    public Transform SpacebarTutorial;
    public SpriteRenderer HeartSprite;
    public TMP_Text HeartCaption;
    public TMP_Text BpmCaption;
    public Transform HelpWidget;
    public TMP_Text HeartStatus;

    public float MaximumTimeBetweenBeatsInMs;
    public float SameBeatMaxDelayInMs;
    public float SameBeatIdealDelayTolerance;
    public float SameBeatGoodDelayTolerance;
    public float SameBeatBadDelayTolerance; 

    public ScriptPrefab DefibSpark;
    public ScriptPrefab TimingText;
    public ScriptPrefab Virus;

    public Vector3 TimingTextSpawn1;
    public Vector3 TimingTextSpawn2;
    public float TimingTextSpawnRadius;

    public bool EnableEffects { get; private set; }
    public bool EnableDefibrillator { get; private set; }
    public bool EnableDeceasing { get; private set; }
    public bool IsBlocked { get; private set; }

    public int BpmTargetTolerance;
    public float BpmUpdateInSeconds;
    public int MinBpmToShow;

    public AudioSource LeftBeat;
    public AudioSource RightBeat;

    public event Action<double> OnSecondBeat;
    public event Action<BeatResult> OnFirstBeat;

    public TMP_Text TargetBpmIndicator;

    public enum BeatResult
    {
        Perfect,
        Good,
        Bad,
        NoResult,
    }

    private Caterpillar<(double, double)> _beats;
    private PlayerInput _playerInput;
    private TextBox _textBox;
    private ColorDefaults _colorDefaults;
    private TimingTextManager _timingTextManager;
    private TimingTextPool _timingTextPool;
    private LifeBar _lifeBar;

    private bool _inTutorial;

    private const string LeftBeatTrigger = "LeftBeat"; // Lub
    private const string RightBeatTrigger = "RightBeat"; // Dub
    private readonly Color Transparent = new Color(0, 0, 0, 0);

    public bool HasHealingBeats { get; set; }
    public bool HasPerfectBeat { get; set; }

    public bool HasBullsHeart { get; set; }

    public void InstantFlatline()
    {
        HandleFlatline();
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        _playerInput = PlayerInput.GetPlayerByIndex(0);
        _textBox = SceneObject<TextBox>.Instance();
        _colorDefaults = SceneObject<ColorDefaults>.Instance();
        _timingTextManager = SceneObject<TimingTextManager>.Instance();
        _timingTextPool = _timingTextManager.GetEffect(TimingText);
        _lifeBar = SceneObject<LifeBar>.Instance(true);
    }

    private void OnEnable()
    {
        _beats = new Caterpillar<(double, double)>
        {
            TailSize = 5
        };
        DefaultMachinery.AddBasicMachine(HandleBeatInput());
        DefaultMachinery.AddBasicMachine(CheckHeartbeat());
    }

    public void SetTargetBpm(int targetBpm)
    {
        TargetBpm = targetBpm;
        TargetBpmText.text = targetBpm == 0 ? "" : $"<color=#d83d3d>{targetBpm}</color> target";

        DefaultMachinery.AddBasicMachine(ShowChangeBpmIndicator());
    }

    private IEnumerable<IEnumerable<Action>> ShowChangeBpmIndicator()
    {
        for (var i = 0; i < 20; i++)
        {
            TargetBpmIndicator.enabled = !TargetBpmIndicator.enabled;
            yield return TimeYields.WaitMilliseconds(GameTimer, 100);
        }

        TargetBpmIndicator.enabled = false;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public IEnumerable<IEnumerable<Action>> EnterShopMode(Func<bool> checkShop, Action onClose)
    {
        EnableDeceasing = false;
        EnableEffects = false;
        IsBlocked = true;

        while (checkShop())
        {
            yield return TimeYields.WaitOneFrameX;
        }

        onClose();

        yield return _textBox.ShowText("Kick start your heart to proceed.", false).AsCoroutine();
        IsBlocked = false;
        while (Flatlined)
        {
            yield return TimeYields.WaitOneFrameX;
        }

        EnableEffects = true;
        EnableDeceasing = true;
    }

    public IEnumerable<IEnumerable<Action>> SkipTutorial()
    {
        EnableEffects = false;
        HeartSprite.enabled = HeartStatus.enabled = HeartCaption.enabled = BpmText.enabled = BpmCaption.enabled = TargetBpmText.enabled = false;
        SpacebarTutorial.gameObject.SetActive(false);
        HelpWidget.gameObject.SetActive(false);

        HeartStatus.text = $"<color=#{_colorDefaults.Healthy.Color.ToHexString()}>Healthy";
        EnableDefibrillator = false;
        EnableDeceasing = false;
        Deceased = false;

        yield return _textBox.ShowText("First, kick start your heart", false).AsCoroutine();

        Defibrillator.GiveItem(2);
        HeartSprite.enabled = HeartStatus.enabled = HeartCaption.enabled = BpmText.enabled = BpmCaption.enabled = TargetBpmText.enabled = true;
        HelpWidget.gameObject.SetActive(true);
        EnableDefibrillator = true;
        EnableDeceasing = true;

        IsBlocked = false;

        // wait for the player to start
        while (Flatlined)
        {
            yield return TimeYields.WaitOneFrameX;
        }

        EnableEffects = true;
        yield return _textBox.ShowText("", false).AsCoroutine();
    }

    public IEnumerable<IEnumerable<Action>> PlayTutorial()
    {
        EnableEffects = false;
        HeartSprite.enabled = HeartStatus.enabled = HeartCaption.enabled = BpmText.enabled = BpmCaption.enabled = TargetBpmText.enabled = false;
        HelpWidget.gameObject.SetActive(false);

        var skipAction = _playerInput.actions[SkipTutorialInput.ActionName];

        yield return _textBox.ShowText("How was the sound of your beating heart?", false).AsCoroutine();
        SpacebarTutorial.gameObject.SetActive(true);
        SkipTutorialHelp.gameObject.SetActive(true);

    checkBeats:
        while (_beats.Length != _beats.TailSize || Bpm == 0)
        {
            if (skipAction.WasPerformedThisFrame())
            {
                SkipTutorialHelp.gameObject.SetActive(false);
                yield return SkipTutorial().AsCoroutine();
                yield break;
            }

            if (_beats.TailSize == 5 && _beats.GetTail(5).Count(b => b.Item2 == 0) >= 3)
            {
                yield return _textBox.ShowText("Heartbeats come in two. Lub Dub. --- pause --- Lub Dub. Now, try again.", false).AsCoroutine();
                _beats.Clear();

                goto checkBeats;
            }

            yield return TimeYields.WaitOneFrameX;
        }

        SpacebarTutorial.gameObject.SetActive(false);
        HelpWidget.gameObject.SetActive(true);
        HeartSprite.enabled = true;

        SkipTutorialHelp.gameObject.SetActive(false);
        _inTutorial = true;

        if (Bpm < 50)
        {
            yield return _textBox.ShowText("To simulate the living, this heart of yours needs to beat a bit faster...", false).AsCoroutine();

            _beats.Clear();

            goto checkBeats;
        }

        if (Bpm > 140)
        {
            yield return _textBox.ShowText("Consider not having a heart attack. I suggest slowing down.", false).AsCoroutine();
            _beats.Clear();

            goto checkBeats;
        }

        BpmCaption.enabled = BpmText.enabled = true;
        yield return _textBox.ShowText("Now, keep going. Would be a mess if you died again, in here of all places.", false).AsCoroutine();

        var flat = false;
        yield return TimeYields.WaitSeconds(GameTimer, 5, step: _ =>
        {
            if (Flatlined)
            {
                flat = true;
            }
        }, () => flat);

        if (flat)
        {
            yield return _textBox
                .ShowText("Oh, once more you died, if only for a moment", false)
                .AsCoroutine();

            yield return TimeYields.WaitSeconds(GameTimer, 2);

            yield return _textBox
                .ShowText("Now let's try again.", false)
                .AsCoroutine();
            _beats.Clear();
            goto checkBeats;
        }

        TargetBpmText.enabled = true;
        SetTargetBpm(80);

        yield return _textBox.ShowText("Good. As human, different activities require different effort.", false).AsCoroutine();
        yield return TimeYields.WaitSeconds(GameTimer, 2);
        yield return _textBox.ShowText("Try adjusting your heart rate to the target. Or close enough.", false).AsCoroutine();

    matchTarget:
        while (_beats.Length != _beats.TailSize || Bpm == 0)
        {
            yield return TimeYields.WaitOneFrameX;
        }

        flat = false;
        yield return TimeYields.WaitSeconds(GameTimer, 10, step: _ =>
        {
            if (Flatlined)
            {
                flat = true;
            }
        }, () => flat);

        if (flat)
        {
            yield return _textBox
                .ShowText("Hey, stop dying on me.", false)
                .AsCoroutine();

            yield return TimeYields.WaitSeconds(GameTimer, 2);

            yield return _textBox
                .ShowText("Once again. Try to adjust your heart rate to the target. Approximately.", false)
                .AsCoroutine();
            goto matchTarget;
        }

        if (!IsAtTargetBpm())
        {
            yield return _textBox.ShowText("At least you're alive, but that's not enough.", false).AsCoroutine();
            yield return TimeYields.WaitSeconds(GameTimer, 2);

            yield return _textBox
                .ShowText("Once again. Try to adjust your heart rate to the target.", false)
                .AsCoroutine();
            goto matchTarget;
        }

        SetTargetBpm(0);

        yield return _textBox.ShowText("Great! Now, as things don't always go as planned...", false).AsCoroutine();
        yield return TimeYields.WaitSeconds(GameTimer, 2);
        yield return _textBox.ShowText("(You're here after all, ain't you)", false).AsCoroutine();
        yield return TimeYields.WaitSeconds(GameTimer, 2);

    defibrillating:

        HeartStatus.text = $"<color=#{_colorDefaults.Healthy.Color.ToHexString()}>Healthy";
        EnableDefibrillator = false;
        EnableDeceasing = false;
        Deceased = false;

        yield return _textBox.ShowText("Flatlining isn't always game over. ", false).AsCoroutine();
        yield return TimeYields.WaitSeconds(GameTimer, 2);

        Defibrillator.GiveItem(1);
        HeartCaption.enabled = true;
        HeartStatus.enabled = true;

        yield return _textBox.ShowText("If you happen to have a defibrillator...", false).AsCoroutine();
        yield return TimeYields.WaitSeconds(GameTimer, 2);

        yield return _textBox.ShowText("It will give you a short time window to kick start your heart again.", false).AsCoroutine();
        yield return TimeYields.WaitSeconds(GameTimer, 2);

        if (Flatlined)
        {
            yield return _textBox.ShowText("By the way, your heart is not beating right now.", false).AsCoroutine();
            yield return TimeYields.WaitSeconds(GameTimer, 2);

            yield return _textBox.ShowText("Start beating again.", false).AsCoroutine();
            yield return TimeYields.WaitSeconds(GameTimer, 2);

            while (Flatlined)
            {
                yield return TimeYields.WaitOneFrameX;
            }
        }

        yield return _textBox.ShowText("Now, give it a shot. Stop beating your heart.", false).AsCoroutine();
        EnableDefibrillator = true;
        EnableDeceasing = true;

        yield return TimeYields.WaitSeconds(GameTimer, 1);
        while (!Flatlined)
        {
            yield return TimeYields.WaitOneFrameX;
        }

        yield return TimeYields.WaitSeconds(GameTimer, 1);
        
        yield return _textBox.ShowText("The defibrillator has been used! Now, start your heart again!", false).AsCoroutine();

        while (Flatlined)
        {
            yield return TimeYields.WaitOneFrameX;
            if (!IsDefibrillating && Flatlined)
            {
                yield return _textBox.ShowText("Oh, you died for good. Again. Let's try it once more.", false).AsCoroutine();
                yield return TimeYields.WaitSeconds(GameTimer, 2);

                goto defibrillating;
            }
        }

        yield return _textBox.ShowText("Done! You can stop for a little bit now.", false).AsCoroutine();
        IsBlocked = true;

        yield return TimeYields.WaitSeconds(GameTimer, 2);

        SetTargetBpm(0);

        yield return _textBox.ShowText("You see the green bar next to your heart?").AsCoroutine();
        yield return _textBox.ShowText("Even an imaginary heart can't beat forever. It has a durability.").AsCoroutine();
        yield return _textBox.ShowText("I'll mend your heart, but you must take good care of it from now on.").AsCoroutine();

        _lifeBar.Heal(_lifeBar.MaximumLife);

        yield return TimeYields.WaitSeconds(GameTimer, 2);

        yield return _textBox.ShowText("Speaking of which, you got to learn how to defend it.").AsCoroutine();
        yield return _textBox.ShowText("You see, an imaginary heart is precious to some of the viruses here.").AsCoroutine();
        yield return _textBox.ShowText("(Yes the purgatory also has viruses, go figure)").AsCoroutine();

        yield return _textBox.ShowText("If by any chance you stumble across one, please melt it.").AsCoroutine();

        HeartLaserHelp.gameObject.SetActive(true);

        if (Virus.Pool.TryGetFromPool(out var virus))
        {
            virus.Component.transform.position = new Vector3(-8f, -3f);

            while (virus.IsActive)
            {
                yield return TimeYields.WaitOneFrameX;
            }
        }

        yield return _textBox.ShowText("That will show them.").AsCoroutine();

        HeartLaserHelp.gameObject.SetActive(false);

        yield return _textBox.ShowText("These are the basics of the purgatory. For now.").AsCoroutine();
        yield return _textBox.ShowText("One last thing. Timing your heart correctly will pay off.").AsCoroutine();
        yield return _textBox.ShowText("You're going to face different challenges, and must act accordingly.").AsCoroutine();
        yield return _textBox.ShowText("To free your mind of this purgatory, your imaginary heart must thrive.").AsCoroutine();
        yield return _textBox.ShowText("Here's a couple defibrillators to help you stay alive.").AsCoroutine();

        Defibrillator.GiveItem(2);
        yield return _textBox.ShowText("I'll also give you some time to kick start your heart again. Good Luck!").AsCoroutine();

        IsBlocked = false;

        // wait for the player to start over
        while (Flatlined)
        {
            yield return TimeYields.WaitOneFrameX;
        }

        EnableEffects = true;
        _inTutorial = false;

        // end of the tutorial, go to game loop
    }

    public bool IsAtTargetBpm()
    {
        return Bpm > TargetBpm - BpmTargetTolerance && Bpm < TargetBpm + BpmTargetTolerance;
    }

    private IEnumerable<IEnumerable<Action>> HandleBeatInput()
    {
        var action = _playerInput.actions[HeartBeatInput.ActionName];
        var secondBeatTime = 0d;
        var beatDelay = 0d;
        while (isActiveAndEnabled)
        {
            if (IsBlocked)
            {
                _beats.Clear();
            }

            while (IsBlocked)
            {
                yield return TimeYields.WaitOneFrameX;
            }

            if (action.WasPerformedThisFrame() && !Deceased)
            {
                var firstBeatTime = GameTimer.TotalElapsedTimeInMilliseconds;

                if (secondBeatTime != 0)
                {
                    var elapsed = firstBeatTime - secondBeatTime;

                    // 20% chance for forced perfect
                    var forcedPerfect = HasPerfectBeat && Random.value <= 0.2f;

                    if (
                        (forcedPerfect && elapsed <= beatDelay + SameBeatBadDelayTolerance &&
                         elapsed >= beatDelay - SameBeatBadDelayTolerance)
                        ||
                        elapsed <= beatDelay + SameBeatIdealDelayTolerance &&
                        elapsed >= beatDelay - SameBeatIdealDelayTolerance
                    )
                    {
                        if (EnableEffects && _timingTextPool.TryGetFromPool(out var effect))
                        {
                            OnFirstBeat?.Invoke(BeatResult.Perfect);
                            effect.TextComponent.text = "perfect";
                            effect.ColorVariations = new[]
                            {
                                _colorDefaults.Healthy.Color,
                                _colorDefaults.Recharging.Color,
                                _colorDefaults.Alert.Color,
                                _colorDefaults.Danger.Color
                            };
                            ;
                            effect.transform.position = (Vector2) (Random.value > 0.5f ? TimingTextSpawn1 : TimingTextSpawn2)
                                                        + Random.insideUnitCircle * TimingTextSpawnRadius;
                        }
                    }
                    else if (elapsed <= beatDelay + SameBeatGoodDelayTolerance &&
                             elapsed >= beatDelay - SameBeatGoodDelayTolerance)
                    {
                        if (EnableEffects && _timingTextPool.TryGetFromPool(out var effect))
                        {
                            OnFirstBeat?.Invoke(BeatResult.Good);
                            effect.TextComponent.text = "good";
                            effect.ColorVariations = new[]
                            {
                                _colorDefaults.Recharging.Color,
                                _colorDefaults.Alert.Color,
                            };
                            ;
                            effect.transform.position = (Vector2)(Random.value > 0.5f ? TimingTextSpawn1 : TimingTextSpawn2)
                                                        + Random.insideUnitCircle * TimingTextSpawnRadius;
                        }
                    }
                    else if (elapsed <= beatDelay + SameBeatBadDelayTolerance &&
                             elapsed >= beatDelay - SameBeatBadDelayTolerance)
                    {
                        if (EnableEffects && _timingTextPool.TryGetFromPool(out var effect))
                        {
                            effect.TextComponent.text = "bad";
                            effect.ColorVariations = new[]
                            {
                                _colorDefaults.Danger.Color
                            };
                            ;
                            effect.transform.position = (Vector2)(Random.value > 0.5f ? TimingTextSpawn1 : TimingTextSpawn2)
                                                        + Random.insideUnitCircle * TimingTextSpawnRadius;
                        }
                        OnFirstBeat?.Invoke(BeatResult.Bad);
                    }
                    else
                    {
                        OnFirstBeat?.Invoke(BeatResult.NoResult);
                    }
                }

                secondBeatTime = 0d;
                Animator.SetTrigger(LeftBeatTrigger);
                if (LeftBeat != null) LeftBeat.Play();
                var secondBeat = false;

                yield return TimeYields.WaitOneFrameX;

                yield return TimeYields.WaitMilliseconds(GameTimer, SameBeatMaxDelayInMs, step: elapsed =>
                {
                    if (action.WasPerformedThisFrame())
                    {
                        Animator.SetTrigger(RightBeatTrigger);
                        secondBeat = true;
                        secondBeatTime = GameTimer.TotalElapsedTimeInMilliseconds;

                        beatDelay = elapsed * 3;
                        OnSecondBeat?.Invoke(beatDelay);


                        if (RightBeat != null) RightBeat.Play();
                    }
                }, () => secondBeat || IsBlocked);

                _beats.Current = (firstBeatTime, secondBeatTime);
            }
            yield return TimeYields.WaitOneFrameX;
        }
    }

    private IEnumerable<IEnumerable<Action>> DefibEffect()
    {
        while (IsDefibrillating)
        {
            const float randomVariation = 360f / 8 * 0.20f;
            if (DefibSpark.Pool.TryGetManyFromPool(8, out var objs))
            {
                for (var i = 0; i < objs.Length; i++)
                {
                    if (Random.value > 0.5f)
                    {
                        yield return TimeYields.WaitMilliseconds(GameTimer, 25);
                        continue;
                    }

                    var obj = objs[i];
                    obj.Component.transform.position = HeartSprite.transform.position;
                    obj.Component.transform.localRotation = Quaternion.identity;
                    obj.Component.transform.Rotate(Vector3.forward, randomVariation + (i * 360f / objs.Length), Space.Self);
                    DefaultMachinery.AddBasicMachine(MoveEffect(obj));
                    yield return TimeYields.WaitMilliseconds(GameTimer, 25);
                }
            }

            yield return TimeYields.WaitMilliseconds(GameTimer, 200);
        }
    }

    private IEnumerable<IEnumerable<Action>> MoveEffect(IPoolableComponent obj)
    {
        while (IsDefibrillating && obj.IsActive)
        {
            obj.Component.transform.Translate(new Vector2(-1,1) * (0.5f + Random.value *0.5f) * (float)GameTimer.UpdatedTimeInMilliseconds * 0.00125f);
            yield return TimeYields.WaitOneFrameX;
        }
    }

    private IEnumerable<IEnumerable<Action>> Defibrillate()
    {
        DefaultMachinery.AddBasicMachine(DefibEffect());
        yield return TimeYields.WaitSeconds(GameTimer, 12, breakCondition: () => !Flatlined);

        yield return TimeYields.WaitOneFrameX;
        IsDefibrillating = false;
        HeartSprite.material.SetColor("_Colorize", Transparent);
        if (!Flatlined)
        {
            HeartStatus.text = $"<color=#{_colorDefaults.Healthy.Color.ToHexString()}>Healthy";
        }
    }

    private IEnumerable<IEnumerable<Action>> CheckHeartbeat()
    {
        while (isActiveAndEnabled)
        {
            var currentTime = GameTimer.TotalElapsedTimeInMilliseconds;
            while (_beats.Length != _beats.TailSize ||
                    currentTime - (_beats.Current.Item1 + _beats.Current.Item2) * 0.5d > MaximumTimeBetweenBeatsInMs)
            {
                BpmText.text = "--";
                Flatlined = true;

                if (_beats.Length == _beats.TailSize && EnableDeceasing)
                {
                    HandleFlatline();
                }

                yield return TimeYields.WaitSeconds(GameTimer, BpmUpdateInSeconds);

                currentTime = GameTimer.TotalElapsedTimeInMilliseconds;
            }

            Bpm = (int)Math.Round(_beats.GetTail(5)
                .Reverse()
                .Select(beat => beat.Item2 == 0 ? beat.Item1 * 0.001d : (beat.Item1 + beat.Item2) * 0.0005d)
                .Pairwise()
                .Sum(pair => 60d / (pair.Item2 - pair.Item1)) / 4d, MidpointRounding.AwayFromZero);

            BpmText.text = Bpm < MinBpmToShow ? "--" : Bpm.ToString();
            Flatlined = Bpm < MinBpmToShow;

            if (Flatlined && EnableDeceasing)
            {
                HandleFlatline();
            }

            yield return TimeYields.WaitSeconds(GameTimer, BpmUpdateInSeconds);
        }
    }

    private void HandleFlatline()
    {
        if (EnableDefibrillator && Defibrillator.UseItem())
        {
            if (IsDefibrillating) return;

            _lifeBar.Heal(Mathf.CeilToInt(_lifeBar.MaximumLife * 0.1f));

            IsDefibrillating = true;
            Flatlined = true;
            Deceased = false;
            _beats.Clear();
            HeartStatus.text = $"<color=#{_colorDefaults.Recharging.Color.ToHexString()}>Defibrillating";
            HeartSprite.material.SetColor("_Colorize", _colorDefaults.Recharging.Color);
            DefaultMachinery.AddBasicMachine(Defibrillate());
        }
        else if (!IsDefibrillating)
        {
            Deceased = true;
            HeartStatus.text = $"<color=#{_colorDefaults.Danger.Color.ToHexString()}>Flatlined";

            if (_inTutorial) return; // if in tutorial, don't show end screen lol

            DefaultMachinery.AddBasicMachine(ShowEndScreen());
        }
    }

    private IEnumerable<IEnumerable<Action>> ShowEndScreen()
    {
        yield return TimeYields.WaitSeconds(GameTimer, 2);
        EndScreen.gameObject.SetActive(true);
    }
}
