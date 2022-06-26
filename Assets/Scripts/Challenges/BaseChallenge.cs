using System;
using System.Collections;
using System.Collections.Generic;
using Licht.Interfaces.Generation;
using Licht.Unity.Objects;
using UnityEngine;

public abstract class BaseChallenge : BaseGameObject, IWeighted<float>
{
    public ScriptColor ActivityColor;
    public string ActivityName;
    public int ChallengeWeight;

    protected Heart Heart;
    protected ChallengeComponents UIComponents;
    protected ColorDefaults ColorDefaults;

    protected sealed override void OnAwake()
    {
        base.OnAwake();
        Heart = SceneObject<Heart>.Instance(true);
        UIComponents = SceneObject<ChallengeComponents>.Instance();
        ColorDefaults = SceneObject<ColorDefaults>.Instance();
    }

    public abstract IEnumerable<IEnumerable<Action>> HandleChallenge();
    public float Weight => ChallengeWeight;
}
