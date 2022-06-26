using System;
using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using UnityEngine;

public class Yoga : BaseChallenge
{
    public int TargetBpm;
    public override IEnumerable<IEnumerable<Action>> HandleChallenge()
    {
        Heart.SetTargetBpm(TargetBpm);
        UIComponents.SetActivity(ActivityName, ColorDefaults.Recharging.Color);

        yield return TimeYields.WaitSeconds(GameTimer, 10);
    }
}
