using System;
using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Unity.Objects;
using UnityEngine;

public class ChallengeHandler : BaseGameObject
{
    private Heart _heart;

    protected override void OnAwake()
    {
        base.OnAwake();
        _heart = SceneObject<Heart>.Instance(true);
    }

    private void OnEnable()
    {
        DefaultMachinery.AddBasicMachine(HandleChallenges());
    }

    private IEnumerable<IEnumerable<Action>> HandleChallenges()
    {
        while (isActiveAndEnabled)
        {
            yield return TimeYields.WaitOneFrameX;
        }
    }
}
