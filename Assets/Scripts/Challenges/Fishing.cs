using System;
using System.Collections.Generic;
using Licht.Impl.Orchestration;

public class Fishing : BaseChallenge
{
    public int TargetBpm1;
    public int TargetBpm2;
    public override IEnumerable<IEnumerable<Action>> HandleChallenge()
    {
        IsActive = true;
        Heart.SetTargetBpm(TargetBpm1);
        yield return TextBox.ShowText($"Starting Activity: {ActivityName}", false).AsCoroutine();
        UIComponents.SetActivity(ActivityName, ColorDefaults.Recharging.Color);

        ActivityClock.SetTimer(50);

        yield return TimeYields.WaitSeconds(GameTimer, 10);
        yield return TextBox.ShowText("Now let's wait for the fish to grab the bait", false).AsCoroutine();

        yield return TimeYields.WaitSeconds(GameTimer, 10);
        yield return TextBox.ShowText("Line... hook...", false).AsCoroutine();

        yield return TimeYields.WaitSeconds(GameTimer, 4);
        yield return TextBox.ShowText("and sink!", false).AsCoroutine();


        Heart.SetTargetBpm(TargetBpm2);
        DefaultMachinery.AddBasicMachine(HandleSpawns());

        Heart.SetTargetBpm(TargetBpm2);

        yield return TimeYields.WaitSeconds(GameTimer, 22);

        yield return TextBox.ShowText($"Finishing Activity: {ActivityName}", false).AsCoroutine();
        yield return TimeYields.WaitSeconds(GameTimer, 2);
        IsActive = false;
        yield return TimeYields.WaitSeconds(GameTimer, 2);
    }
}