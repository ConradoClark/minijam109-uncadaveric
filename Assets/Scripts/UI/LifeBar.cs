using System;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Unity.Builders;
using Licht.Unity.Objects;
using TMPro;
using UnityEngine;

public class LifeBar : BaseUIObject
{
    public int MaximumLife;
    public int CurrentLife { get; private set; }
    private Heart _heart;

    private bool _updating;

    public TMP_Text BarText;
    public SpriteRenderer BarBorder;
    public SpriteRenderer BarSprite;
    public float BarSpriteMaxSize;

    public void EnableUI()
    {
        BarBorder.enabled = BarSprite.enabled = BarText.enabled = true;
    }

    public void DisableUI()
    {
        BarBorder.enabled = BarSprite.enabled = BarText.enabled = false;
    }

    protected override void OnAwake()
    {
        CurrentLife = MaximumLife;
        BarSprite.size = new Vector2(GetTargetSize(), BarSprite.size.y);
        BarText.text = $"{CurrentLife} / {MaximumLife}";
        _heart = SceneObject<Heart>.Instance();

        
    }

    private void OnEnable()
    {
        _heart.OnSecondBeat += Heart_OnSecondBeat;
        DefaultMachinery.AddBasicMachine(HandleUI());
    }

    private void OnDisable()
    {
        _heart.OnSecondBeat -= Heart_OnSecondBeat;
    }

    private IEnumerable<IEnumerable<Action>> HandleUI()
    {
        while (isActiveAndEnabled)
        {
            if (!_heart.EnableDeceasing || !_heart.enabled) DisableUI();
            else if (!BarBorder.enabled) EnableUI();


            yield return TimeYields.WaitMilliseconds(UITimer, 100);
        }
    }

    private void Heart_OnSecondBeat(double obj)
    {
        if (_heart.Flatlined || _heart.IsDefibrillating) return;

        CurrentLife -= _heart.Bpm;
        if (CurrentLife <= 0) CurrentLife = 0;
        BarText.text = $"{CurrentLife} / {MaximumLife}";

        DefaultMachinery.AddBasicMachine(UpdateBar());
    }

    private IEnumerable<IEnumerable<Action>> UpdateBar()
    {
        if (_updating)
        {
            _updating = false;
            yield return TimeYields.WaitOneFrameX;
        }

        _updating = true;

        if (_heart.Bpm > 110)
        {
            BarSprite.size = new Vector2(GetTargetSize(), BarSprite.size.y);
        }
        else
        {
            yield return new LerpBuilder(f => BarSprite.size = new Vector2(f, BarSprite.size.y),
                    () => BarSprite.size.x)
                .SetTarget(GetTargetSize)
                .Over(0.35f)
                .BreakIf(() => !_updating, false)
                .Easing(EasingYields.EasingFunction.QuadraticEaseInOut)
                .UsingTimer(UITimer)
                .Build();
        }

        _updating = false;
    }

    private float GetTargetSize()
    {
        return Mathf.Clamp((float)CurrentLife / MaximumLife * BarSpriteMaxSize, 0, BarSpriteMaxSize);
    }
}
