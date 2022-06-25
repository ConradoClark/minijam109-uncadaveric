using System;
using System.Collections.Generic;
using System.Linq;
using Licht.Impl.Memory;
using Licht.Impl.Orchestration;
using Licht.Unity.Objects;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Heart : BaseGameObject
{
    public ScriptInput HeartBeatInput;
    public int Bpm { get; private set; }
    public int TargetBpm { get; private set; }
    public bool Flatlined { get; private set; }
    
    public Animator Animator;
    public TMP_Text TargetBpmText;
    public TMP_Text BpmText;

    public Transform SpacebarTutorial;
    public SpriteRenderer HeartSprite;
    public TMP_Text HeartCaption;
    public TMP_Text BpmCaption;
    public Transform HelpWidget;

    public float MaximumTimeBetweenBeatsInMs;
    public float SameBeatMaxDelayInMs;
    public float SameBeatIdealDelayInMs;
    public float SameBeatIdealDelayTolerance;
    public float SameBeatGoodDelayTolerance;
    public float SameBeatBadDelayTolerance;

    public bool IsBlocked { get; private set; }

    public int BpmTargetTolerance;
                              
    public float BpmUpdateInSeconds;
    public int MinBpmToShow;

    public AudioSource LeftBeat;
    public AudioSource RightBeat;

    private Caterpillar<(double, double)> _beats;
    private PlayerInput _playerInput;
    private TextBox _textBox;

    private const string LeftBeatTrigger = "LeftBeat"; // Lub
    private const string RightBeatTrigger = "RightBeat"; // Dub

    protected override void OnAwake()
    {
        base.OnAwake();
        _playerInput = PlayerInput.GetPlayerByIndex(0);
        _textBox = SceneObject<TextBox>.Instance();
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

    public IEnumerable<IEnumerable<Action>> PlayTutorial()
    {
        HeartSprite.enabled = HeartCaption.enabled = BpmText.enabled = BpmCaption.enabled = TargetBpmText.enabled = false;
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
            _beats.Clear();
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

        yield return _textBox.ShowText("Done! You can stop for a little bit now.",false).AsCoroutine();
        IsBlocked = true;

        yield return TimeYields.WaitSeconds(GameTimer, 2);

        HeartCaption.enabled = true;
        SetTargetBpm(0);
        yield return _textBox.ShowText("These are the basics of the purgatory. For now.").AsCoroutine();
        yield return _textBox.ShowText("You're going to face different challenges, and must act accordingly.").AsCoroutine();
        yield return _textBox.ShowText("I'll give you some time to kick start your heart again.").AsCoroutine();

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

            if (action.WasPerformedThisFrame())
            {
                var firstBeatTime = GameTimer.TotalElapsedTimeInMilliseconds;
                var secondBeatTime = 0d;
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

                        if (RightBeat != null) RightBeat.Play();

                        if (elapsed <= SameBeatIdealDelayInMs + SameBeatIdealDelayTolerance &&
                            elapsed >= SameBeatIdealDelayInMs - SameBeatIdealDelayTolerance)
                        {
                            Debug.Log("perfect");
                            // perfect
                        }
                        else if (elapsed <= SameBeatIdealDelayInMs + SameBeatGoodDelayTolerance &&
                                 elapsed >= SameBeatIdealDelayInMs - SameBeatGoodDelayTolerance)
                        {
                            Debug.Log("good");
                            // good
                        }
                        else if (elapsed <= SameBeatIdealDelayInMs + SameBeatBadDelayTolerance &&
                                 elapsed >= SameBeatIdealDelayInMs - SameBeatBadDelayTolerance)
                        {
                            Debug.Log("bad");
                            // bad
                        }
                        // failed beat (what to do in this case?)

                    }
                }, () => secondBeat || IsBlocked);

                _beats.Current = (firstBeatTime, secondBeatTime);
            }
            yield return TimeYields.WaitOneFrameX;
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
                yield return TimeYields.WaitSeconds(GameTimer, BpmUpdateInSeconds);

                currentTime = GameTimer.TotalElapsedTimeInMilliseconds;
            }

            Bpm = (int)Math.Round(_beats.GetTail(5)
                .Reverse()
                .Select(beat => (beat.Item1 + beat.Item2) * 0.0005d)
                .Pairwise()
                .Sum(pair => 60d / (pair.Item2 - pair.Item1)) / 4d, MidpointRounding.AwayFromZero);

            BpmText.text = Bpm < MinBpmToShow ? "--" : Bpm.ToString();
            Flatlined = Bpm < MinBpmToShow;

            yield return TimeYields.WaitSeconds(GameTimer, BpmUpdateInSeconds);
        }
    }
}
