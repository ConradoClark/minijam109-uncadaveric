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
    public int CompletionBonus;
    public bool Completed { get; protected set; }

    protected Heart Heart;
    protected ChallengeComponents UIComponents;

    protected sealed override void OnAwake()
    {
        base.OnAwake();
        Heart = SceneObject<Heart>.Instance(true);
        UIComponents = SceneObject<ChallengeComponents>.Instance();
    }

    public abstract IEnumerable<IEnumerable<Action>> HandleChallenge();
    public float Weight => ChallengeWeight;
}
