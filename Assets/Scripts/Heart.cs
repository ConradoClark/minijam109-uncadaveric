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
    public Animator Animator;
    public TMP_Text TargetBpmText;
    public TMP_Text BpmText;

    public float MaximumTimeBetweenBeatsInMs;
    public float SameBeatMaxDelayInMs;
    public float SameBeatIdealDelayInMs;
    public float SameBeatIdealDelayTolerance;
    public float SameBeatGoodDelayTolerance;
    public float SameBeatBadDelayTolerance;

    public float BpmUpdateInSeconds;
    public int MinBpmToShow;

    private Caterpillar<(double, double)> _beats;
    private PlayerInput _playerInput;

    private const string LeftBeatTrigger = "LeftBeat"; // Lub
    private const string RightBeatTrigger = "RightBeat"; // Dub

    protected override void OnAwake()
    {
        base.OnAwake();
        _playerInput = PlayerInput.GetPlayerByIndex(0);
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
        TargetBpmText.text = $"<color=#d83d3d>{targetBpm}</color> target";
    }

    private IEnumerable<IEnumerable<Action>> HandleBeatInput()
    {
        var action = _playerInput.actions[HeartBeatInput.ActionName];
        while (isActiveAndEnabled)
        {
            if (action.WasPerformedThisFrame())
            {
                var firstBeatTime = GameTimer.TotalElapsedTimeInMilliseconds;
                var secondBeatTime = 0d;
                Animator.SetTrigger(LeftBeatTrigger);
                var secondBeat = false;

                yield return TimeYields.WaitOneFrameX;

                yield return TimeYields.WaitMilliseconds(GameTimer, SameBeatMaxDelayInMs, step: elapsed =>
                {
                    if (action.WasPerformedThisFrame())
                    {
                        Animator.SetTrigger(RightBeatTrigger);
                        secondBeat = true;
                        secondBeatTime = GameTimer.TotalElapsedTimeInMilliseconds;

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
                }, () => secondBeat);

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
                yield return TimeYields.WaitSeconds(GameTimer, BpmUpdateInSeconds);

                currentTime = GameTimer.TotalElapsedTimeInMilliseconds;
            }

            Bpm = (int)Math.Round(_beats.GetTail(5)
                .Reverse()
                .Select(beat => (beat.Item1 + beat.Item2) * 0.0005d)
                .Pairwise()
                .Sum(pair => 60d / (pair.Item2 - pair.Item1)) / 4d, MidpointRounding.AwayFromZero);

            BpmText.text = Bpm < MinBpmToShow ? "--" : Bpm.ToString();

            yield return TimeYields.WaitSeconds(GameTimer, BpmUpdateInSeconds);
        }
    }
}
