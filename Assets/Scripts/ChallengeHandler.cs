using System;
using System.Collections.Generic;
using Licht.Impl.Generation;
using Licht.Impl.Orchestration;
using Licht.Interfaces.Generation;
using Licht.Interfaces.Update;
using Licht.Unity.Objects;
using Random = UnityEngine.Random;

public class ChallengeHandler : BaseGameObject, IGenerator<int,float>, IActivable
{
    public BaseChallenge[] Challenges;

    private Heart _heart;
    public bool IsActive { get; private set; }
    public bool Activate()
    {
        if (IsActive) return false;

        IsActive = true;
        return true;
    }

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
        var dice = new WeightedDice<BaseChallenge>(Challenges, this);
        while (isActiveAndEnabled)
        {
            while (!IsActive)
            {
                yield return TimeYields.WaitOneFrameX;
            }

            var challenge = dice.Generate();
            yield return challenge.HandleChallenge().AsCoroutine();

            yield return TimeYields.WaitOneFrameX;
        }
    }

    public int Seed { get; set; }
    public float Generate()
    {
        return Random.value;
    }
}
