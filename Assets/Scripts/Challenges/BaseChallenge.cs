using System;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Interfaces.Generation;
using Licht.Unity.Objects;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class BaseChallenge : BaseGameObject, IWeighted<float>
{
    public ScriptColor ActivityColor;
    public string ActivityName;
    public int ChallengeWeight;
    public ScriptPrefab Virus;
    public ScriptPrefab VirusDeath;

    protected Heart Heart;
    protected ChallengeComponents UIComponents;
    protected ColorDefaults ColorDefaults;
    protected TextBox TextBox;
    protected ActivityClock ActivityClock;
    protected ChallengeHandler ChallengeHandler;
    protected bool IsActive;

    public float MinDelayBetweenVirusInSeconds;
    public float MaxDelayBetweenVirusInSeconds;
    public float MinVirusDensity;
    public float MaxVirusDensity;

    public int LevelMin = 0;
    public int LevelMax = 999;

    protected sealed override void OnAwake()
    {
        base.OnAwake();
        Heart = SceneObject<Heart>.Instance(true);
        UIComponents = SceneObject<ChallengeComponents>.Instance();
        ColorDefaults = SceneObject<ColorDefaults>.Instance();
        TextBox = SceneObject<TextBox>.Instance();
        ActivityClock = SceneObject<ActivityClock>.Instance();
        ChallengeHandler = SceneObject<ChallengeHandler>.Instance();
    }

    protected virtual IEnumerable<IEnumerable<Action>> HandleSpawns()
    {
        while (IsActive)
        {
            yield return TimeYields.WaitSeconds(GameTimer,
                Random.Range(MinDelayBetweenVirusInSeconds, MaxDelayBetweenVirusInSeconds), breakCondition: ()=> !IsActive);

            if (!IsActive) break;

            var density = Random.Range(MinVirusDensity, MaxVirusDensity);
            var mixedSide = Random.value <= 0.5f;
            var side = Random.value <= 0.5f ? UIComponents.VirusSpawnLeft : UIComponents.VirusSpawnRight;


            if (Virus.Pool.TryGetManyFromPool(Mathf.CeilToInt(density), out var enemies))
            {
                foreach (var enemy in enemies)
                {
                    enemy.Component.transform.position = (Vector2)side + Random.insideUnitCircle * 1.5f;

                    if (mixedSide)
                    {
                        side = Random.value <= 0.5f ? UIComponents.VirusSpawnLeft : UIComponents.VirusSpawnRight;
                    }
                }
            }
        }

        if (ChallengeHandler.ShopIn == 1)
        {
            foreach (var v in Virus.Pool.GetObjectsInUse())
            {
                if (!v.Component.isActiveAndEnabled) continue;
                if (VirusDeath.Pool.TryGetFromPool(out var effect))
                {
                    effect.Component.transform.position = v.Component.transform.position;
                }
            }

            Virus.Pool.ReleaseAll();
        }
    }

    public abstract IEnumerable<IEnumerable<Action>> HandleChallenge();
    public float Weight => ChallengeWeight;
}
