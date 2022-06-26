using System;
using Licht.Unity.Objects;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Unity.Extensions;
using UnityEngine;

public class TargetIndicator : BaseUIObject
{
    private Vector3 _originalPosition;
    private Heart _heart;

    public Sprite UpIndicator;
    public Sprite DownIndicator;
    public Sprite OkIndicator;

    public SpriteRenderer SpriteRenderer;

    protected override void OnAwake()
    {
        base.OnAwake();
        _originalPosition = transform.position;
        _heart = SceneObject<Heart>.Instance(true);
    }

    private void OnEnable()
    {
        DefaultMachinery.AddBasicMachine(CheckBpm());
        DefaultMachinery.AddBasicMachine(Movement());
    }

    private IEnumerable<IEnumerable<Action>> CheckBpm()
    {
        while (isActiveAndEnabled)
        {
            if (_heart.TargetBpm == 0 || _heart.Flatlined || _heart.IsDefibrillating)
            {
                SpriteRenderer.enabled = false;
            }
            else if (_heart.IsAtTargetBpm())
            {
                SpriteRenderer.enabled = true;
                SpriteRenderer.sprite = OkIndicator;
            }
            else if (_heart.Bpm > _heart.TargetBpm)
            {
                SpriteRenderer.enabled = true;
                SpriteRenderer.sprite = DownIndicator;
            }
            else
            {
                SpriteRenderer.enabled = true;
                SpriteRenderer.sprite = UpIndicator;
            }

            yield return TimeYields.WaitMilliseconds(UITimer, 100);
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
