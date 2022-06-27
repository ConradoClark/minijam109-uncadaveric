using System;
using System.Collections.Generic;
using Licht.Impl.Orchestration;

public class Yoga : BaseChallenge
{
    public int TargetBpm;
    public override IEnumerable<IEnumerable<Action>> HandleChallenge()
    {
        IsActive = true;
        Heart.SetTargetBpm(TargetBpm);
        yield return TextBox.ShowText($"Starting Activity: {ActivityName}", false).AsCoroutine();
        UIComponents.SetActivity(ActivityName, ColorDefaults.Recharging.Color);

        ActivityClock.SetTimer(20);

        DefaultMachinery.AddBasicMachine(HandleSpawns());

        yield return TimeYields.WaitSeconds(GameTimer, 16);
        yield return TextBox.ShowText($"Finishing Activity: {ActivityName}", false).AsCoroutine();
        yield return TimeYields.WaitSeconds(GameTimer, 2);
        IsActive = false;
        yield return TimeYields.WaitSeconds(GameTimer, 2);
    }
}
