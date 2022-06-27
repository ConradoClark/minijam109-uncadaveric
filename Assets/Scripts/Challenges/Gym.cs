using System;
using System.Collections.Generic;
using Licht.Impl.Orchestration;

public class Gym : BaseChallenge
{
    public int TargetBpm1;
    public int TargetBpm2;
    public int TargetBpm3;
    public override IEnumerable<IEnumerable<Action>> HandleChallenge()
    {
        IsActive = true;
        Heart.SetTargetBpm(TargetBpm1);
        yield return TextBox.ShowText($"Starting Activity: {ActivityName}", false).AsCoroutine();
        UIComponents.SetActivity(ActivityName, ColorDefaults.Recharging.Color);

        ActivityClock.SetTimer(60);

        DefaultMachinery.AddBasicMachine(HandleSpawns());

        yield return TimeYields.WaitSeconds(GameTimer, 4);
        yield return TextBox.ShowText("Keep going, keep going", false).AsCoroutine();

        yield return TimeYields.WaitSeconds(GameTimer, 4);
        yield return TextBox.ShowText("I want to see you sweat!", false).AsCoroutine();

        Heart.SetTargetBpm(TargetBpm2);

        yield return TimeYields.WaitSeconds(GameTimer, 10);
        yield return TextBox.ShowText("Now, intensify!", false).AsCoroutine();

        Heart.SetTargetBpm(TargetBpm3);

        yield return TimeYields.WaitSeconds(GameTimer, 8);
        yield return TextBox.ShowText("Rest a little bit...", false).AsCoroutine();

        Heart.SetTargetBpm(TargetBpm2);

        yield return TimeYields.WaitSeconds(GameTimer, 10);
        yield return TextBox.ShowText("Now, intensify!", false).AsCoroutine();

        Heart.SetTargetBpm(TargetBpm3);

        yield return TimeYields.WaitSeconds(GameTimer, 8);
        yield return TextBox.ShowText("We're almost done, keep up!", false).AsCoroutine();

        Heart.SetTargetBpm(TargetBpm2);

        yield return TimeYields.WaitSeconds(GameTimer,6 );
        yield return TextBox.ShowText("Now slow down, breathe, breathe...", false).AsCoroutine();

        Heart.SetTargetBpm(TargetBpm1);

        yield return TimeYields.WaitSeconds(GameTimer, 6);
        yield return TextBox.ShowText($"Finishing Activity: {ActivityName}", false).AsCoroutine();
        yield return TimeYields.WaitSeconds(GameTimer, 2);
        IsActive = false;
        yield return TimeYields.WaitSeconds(GameTimer, 2);
    }
}