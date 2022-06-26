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
    public ScriptInput HeartBeatInput;
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

    private const string LeftBeatTrigger = "LeftBeat"; // Lub
    private const string RightBeatTrigger = "RightBeat"; // Dub
    private readonly Color Transparent = new Color(0, 0, 0, 0);

    protected override void OnAwake()
    {
        base.OnAwake();
        _playerInput = PlayerInput.GetPlayerByIndex(0);
        _textBox = SceneObject<TextBox>.Instance();
        _colorDefaults = SceneObject<ColorDefaults>.Instance();
        _timingTextManager = SceneObject<TimingTextManager>.Instance();
        _timingTextPool = _timingTextManager.GetEffect(TimingText);
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
    }

    public void Activate()
    {
        gameObject.SetActive(true);
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
        SpacebarTutorial.gameObject.SetActive(true);
        HelpWidget.gameObject.SetActive(false);
        yield return _textBox.ShowText("How was the sound of your beating heart?", false).AsCoroutine();

    checkBeats:
        while (_beats.Length != _beats.TailSize || Bpm == 0)
        {
            yield return TimeYields.WaitOneFrameX;
        }

        SpacebarTutorial.gameObject.SetActive(false);
        HelpWidget.gameObject.SetActive(true);
        HeartSprite.enabled = true;

        Debug.Log("Current BPM: " + Bpm);

        if (Bpm < 50)
        {
            yield return _textBox.ShowText("To simulate the living, this heart of yours needs to beat a bit faster...", false).AsCoroutine();

            _beats.Clear();

            goto checkBeats;
        }


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
            if (!IsDefibrillating)
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
        yield return _textBox.ShowText("These are the basics of the purgatory. For now.").AsCoroutine();
        yield return _textBox.ShowText("One last thing. Timing your heart correctly will pay off.").AsCoroutine();
        yield return _textBox.ShowText("You're going to face different challenges, and must act accordingly.").AsCoroutine();
        yield return _textBox.ShowText("Here's a couple defibrillators. Please don't die on me now.").AsCoroutine();

        Defibrillator.GiveItem(2);
        yield return _textBox.ShowText("I'll also give you some time to kick start your heart again. Good Luck!").AsCoroutine();

        IsBlocked = false;

        // wait for the player to start over
        while (Flatlined)
        {
            yield return TimeYields.WaitOneFrameX;
        }

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
                    if (elapsed <= beatDelay + SameBeatIdealDelayTolerance &&
                        elapsed >= beatDelay - SameBeatIdealDelayTolerance)
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
            // used defibrillator
            Debug.Log("used defibrillator");
            IsDefibrillating = true;
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
        }
    }
}
