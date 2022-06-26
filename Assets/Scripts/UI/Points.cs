using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Licht.Impl.Orchestration;
using Licht.Unity.Extensions;
using Licht.Unity.Objects;
using TMPro;
using UnityEngine;

public class Points : BaseUIObject
{
    public int NumberOfDigits;
    public TMP_Text PointsText;
    public int CurrentScore;
    public int TotalScore;

    private bool _effectInProgress;

    public void AddPoints(int points)
    {
        CurrentScore += points;
        TotalScore += points;
        UpdateText(true);
    }

    private IEnumerable<IEnumerable<Action>> AddPointsEffect()
    {
        if (_effectInProgress)
        {
            _effectInProgress = false;
            yield return TimeYields.WaitOneFrameX;
        }

        _effectInProgress = true;

        yield return PointsText.transform.GetAccessor()
            .LocalScale
            .Y
            .Increase(0.35f)
            .Over(0.15f)
            .Easing(EasingYields.EasingFunction.CubicEaseOut)
            .BreakIf(() => !_effectInProgress)
            .UsingTimer(UITimer)
            .Build();

        yield return PointsText.transform.GetAccessor()
            .LocalScale
            .Y
            .Decrease(0.35f)
            .Over(0.15f)
            .Easing(EasingYields.EasingFunction.CubicEaseIn)
            .BreakIf(() => !_effectInProgress)
            .UsingTimer(UITimer)
            .Build();

        _effectInProgress = false;
        PointsText.transform.localScale = Vector3.one;
    }

    public bool SpendPoints(int points) 
    {
        if (CurrentScore < points) return false;
        CurrentScore -= points;
        UpdateText();
        return true;
    }

    private void UpdateText(bool useEffect = false)
    {
        PointsText.text = CurrentScore.ToString().PadLeft(NumberOfDigits, '0');
        if (useEffect)
        {
            DefaultMachinery.AddBasicMachine(AddPointsEffect());
        }
    }
}

