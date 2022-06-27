using System;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Unity.Extensions;
using Licht.Unity.Objects;
using TMPro;
using UnityEngine;

public class ActivityClock : BaseUIObject
{
    public TMP_Text TimerText;
    public SpriteRenderer SpriteRenderer;
    private Vector3 _originalPosition;
    private int TimerInSeconds;

    public void SetTimer(int seconds)
    {
        TimerInSeconds = seconds;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        _originalPosition = transform.position;
    }

    private void OnEnable()
    {
        DefaultMachinery.AddBasicMachine(HandleTimer());
        DefaultMachinery.AddBasicMachine(Movement());
    }

    private IEnumerable<IEnumerable<Action>> HandleTimer()
    {
        while (isActiveAndEnabled)
        {
            if( TimerInSeconds == 0)
            {
                SpriteRenderer.enabled = false;
                TimerText.text = "";
            }

            while (TimerInSeconds == 0)
            {
                yield return TimeYields.WaitOneFrameX;
            }

            SpriteRenderer.enabled = true;
            var currentTimer = TimerInSeconds;
            yield return TimeYields.WaitSeconds(UITimer, TimerInSeconds, elapsed =>
            {
                TimerText.text = Mathf.CeilToInt(Mathf.Clamp(currentTimer - (float) elapsed * 0.001f, 0, currentTimer)).ToString();
            }, () => TimerInSeconds != currentTimer);

            if (currentTimer == TimerInSeconds) TimerInSeconds = 0;
        }
    }

    private IEnumerable<IEnumerable<Action>> Movement()
    {
        transform.position = _originalPosition;
        while (isActiveAndEnabled)
        {
            yield return transform.GetAccessor()
                .Position
                .Y
                .Increase(0.05f)
                .Over(0.5f)
                .Easing(EasingYields.EasingFunction.QuadraticEaseInOut)
                .UsingTimer(UITimer)
                .Build();

            yield return transform.GetAccessor()
                .Position
                .Y
                .Decrease(0.05f)
                .Over(0.5f)
                .Easing(EasingYields.EasingFunction.QuadraticEaseInOut)
                .UsingTimer(UITimer)
                .Build();
        }
    }
}

