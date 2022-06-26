using System;
using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Unity.Extensions;
using Licht.Unity.Objects;
using UnityEngine;

public class BeatLine : BaseGameObject
{
    public SpriteRenderer BeatLineSprite;
    public SpriteRenderer BeatLineMark;
    public SpriteRenderer BeatLineTarget;

    private Heart _heart;
    private ColorDefaults _colorDefaults;
    private Points _points;

    private const float PerfectBeatMultiplier = 100;
    private const float GoodBeatMultiplier = 50;
    private const float BadBeatMultiplier = 10;

    protected override void OnAwake()
    {
        base.OnAwake();
        _heart = SceneObject<Heart>.Instance(true);
        _colorDefaults = SceneObject<ColorDefaults>.Instance();
        _points = SceneObject<Points>.Instance();
    }

    private void OnEnable()
    {
        _heart.OnSecondBeat += Heart_OnSecondBeat;
        _heart.OnFirstBeat += Heart_OnFirstBeat;
        DefaultMachinery.AddBasicMachine(HandleBeatLine());
        DefaultMachinery.AddBasicMachine(HandleMarkMovement());
    }

    private void OnDisable()
    {
        _heart.OnSecondBeat -= Heart_OnSecondBeat;
        _heart.OnFirstBeat -= Heart_OnFirstBeat;
    }

    private IEnumerable<IEnumerable<Action>> HandleBeatLine()
    {
        while (isActiveAndEnabled)
        {
            BeatLineSprite.enabled = BeatLineMark.enabled = BeatLineTarget.enabled = _heart.EnableDeceasing && !_heart.Flatlined && !_heart.IsDefibrillating;
            yield return TimeYields.WaitMilliseconds(GameTimer, 100);
        }
    }

    private void Heart_OnFirstBeat(Heart.BeatResult obj)
    {
        if (_heart.Flatlined) return;

        switch (obj)
        {
            case Heart.BeatResult.Perfect:
                var effect1 = BeatLineTarget.GetAccessor()
                    .Material("_Colorize")
                    .AsColor()
                    .ToColor(Color.white)
                    .Over(0.25f)
                    .Easing(EasingYields.EasingFunction.QuadraticEaseOut)
                    .UsingTimer(GameTimer)
                    .Build();
                var effect2 = BeatLineTarget.GetAccessor()
                    .Material("_Colorize")
                    .AsColor()
                    .ToColor(new Color(0, 0, 0, 0))
                    .Over(0.25f)
                    .Easing(EasingYields.EasingFunction.QuadraticEaseIn)
                    .UsingTimer(GameTimer)
                    .Build();
                DefaultMachinery.AddBasicMachine(effect1.Then(effect2));
                _points.AddPoints(_heart.TargetBpm > 0
                    ? Mathf.CeilToInt(PerfectBeatMultiplier *
                                      (1.5f * _heart.BpmTargetTolerance - Mathf.Min(Mathf.Abs(_heart.Bpm - _heart.TargetBpm), _heart.BpmTargetTolerance)))
                    : Mathf.CeilToInt(PerfectBeatMultiplier * 3f));


                break;
            case Heart.BeatResult.Good:
                var eff1 = BeatLineTarget.GetAccessor()
                    .Material("_Colorize")
                    .AsColor()
                    .ToColor(_colorDefaults.Recharging.Color)
                    .Over(0.25f)
                    .Easing(EasingYields.EasingFunction.QuadraticEaseOut)
                    .UsingTimer(GameTimer)
                    .Build();
                var eff2 = BeatLineTarget.GetAccessor()
                    .Material("_Colorize")
                    .AsColor()
                    .ToColor(new Color(0, 0, 0, 0))
                    .Over(0.25f)
                    .Easing(EasingYields.EasingFunction.QuadraticEaseIn)
                    .UsingTimer(GameTimer)
                    .Build();
                DefaultMachinery.AddBasicMachine(eff1.Then(eff2));
                _points.AddPoints(_heart.TargetBpm > 0
                    ? Mathf.CeilToInt(GoodBeatMultiplier *
                                      (1.5f * _heart.BpmTargetTolerance - Mathf.Min(Mathf.Abs(_heart.Bpm - _heart.TargetBpm), _heart.BpmTargetTolerance)))
                    : Mathf.CeilToInt(GoodBeatMultiplier * 3f));
                break;
            case Heart.BeatResult.Bad:
                _points.AddPoints(_heart.TargetBpm > 0
                    ? Mathf.CeilToInt(BadBeatMultiplier *
                                      (1.5f * _heart.BpmTargetTolerance - Mathf.Min(Mathf.Abs(_heart.Bpm - _heart.TargetBpm), _heart.BpmTargetTolerance)))
                    : Mathf.CeilToInt(BadBeatMultiplier * 3f));
                break;
            default:
                break;
        }
    }

    private void Heart_OnSecondBeat(double obj)
    {
        if (_heart.Flatlined) return;
        BeatLineMark.transform.localPosition = new Vector3(-1, 0, 0);
        BeatLineTarget.transform.localPosition = new Vector3((float)obj / 1000f - 1 + 0.0125f, 0, 0);
    }

    private IEnumerable<IEnumerable<Action>> HandleMarkMovement()
    {
        while (isActiveAndEnabled)
        {
            while (_heart.Flatlined)
            {
                yield return TimeYields.WaitOneFrameX;
            }

            BeatLineMark.transform.localPosition =
                new Vector3(Mathf.Clamp(BeatLineMark.transform.localPosition.x
                    + (float)GameTimer.UpdatedTimeInMilliseconds * 0.001f
                    , -1, 1), 0, 0);

            yield return TimeYields.WaitOneFrameX;
        }
    }


}
