using System;
using System.Collections.Generic;
using Licht.Impl.Orchestration;

public class Yoga : BaseChallenge
{
    public int TargetBpm;
    public override IEnumerable<IEnumerable<Action>> HandleChallenge()
    {
        yield return TextBox.ShowText($"Starting Activity: {ActivityName}", false).AsCoroutine();
        Heart.SetTargetBpm(TargetBpm);
        UIComponents.SetActivity(ActivityName, ColorDefaults.Recharging.Color);

        ActivityClock.SetTimer(20);

        yield return TimeYields.WaitSeconds(GameTimer, 16);
        yield return TextBox.ShowText($"Finishing Activity: {ActivityName}", false).AsCoroutine();
        yield return TimeYields.WaitSeconds(GameTimer, 4);
    }
}
