using System;
using System.Collections;
using System.Collections.Generic;
using Licht.Unity.Objects;
using UnityEngine;

public abstract class BaseChallenge : BaseGameObject
{
    protected Heart Heart;

    public ScriptColor ActivityColor;
    public string ActivityName;

    public int CompletionBonus;
    public bool Completed { get; protected set; }
    protected sealed override void OnAwake()
    {
        base.OnAwake();
        Heart = SceneObject<Heart>.Instance(true);
    }

    public abstract IEnumerable<IEnumerable<Action>> HandleChallenge();


}
