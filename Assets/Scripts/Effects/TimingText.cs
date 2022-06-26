using System;
using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Interfaces.Time;
using Licht.Unity.Extensions;
using Licht.Unity.Objects;
using Licht.Unity.Pooling;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class TimingText : EffectPoolable
{
    public TMP_Text TextComponent;
    public float DurationInSeconds = 0.8f;
    public Color[] ColorVariations;
    private ITimer _uiTimer;
    protected override void OnAwake()
    {
        base.OnAwake();
        _uiTimer = SceneObject<DefaultUITimer>.Instance().TimerRef.Timer;
    }

    public override void OnActivation()
    {
        DefaultMachinery.AddBasicMachine(HandleDuration());
        DefaultMachinery.AddBasicMachine(HandleTextEffect());
        DefaultMachinery.AddBasicMachine(HandleMovement());
    }

    public override bool IsEffectOver { get; protected set; }

    private IEnumerable<IEnumerable<Action>> HandleDuration()
    {
        transform.localScale = Vector3.one;

        yield return TimeYields.WaitSeconds(_uiTimer, DurationInSeconds * 0.8f);

        yield return transform.GetAccessor()
            .LocalScale
            .Y
            .SetTarget(0.01f)
            .Over(DurationInSeconds * 0.2f)
            .Easing(EasingYields.EasingFunction.QuadraticEaseOut)
            .UsingTimer(_uiTimer)
            .Build();

        IsEffectOver = true;
    }

    private IEnumerable<IEnumerable<Action>> HandleTextEffect()
    {
        yield return TimeYields.WaitOneFrameX;

        var i = 0;
        while (!IsEffectOver && ColorVariations.Length>0)
        {
            var color = ColorVariations[i % ColorVariations.Length];
            TextComponent.color = color;
            i++;
            yield return TimeYields.WaitMilliseconds(_uiTimer, 50, breakCondition: () => IsEffectOver);
        }
    }

    private IEnumerable<IEnumerable<Action>> HandleMovement()
    {
        var amount = 0.0002f + Random.value * 0.0002f;

        while (!IsEffectOver)
        {   
            transform.position += new Vector3(0, amount * (float)_uiTimer.UpdatedTimeInMilliseconds);
            yield return TimeYields.WaitOneFrameX;
        }
    }
}
