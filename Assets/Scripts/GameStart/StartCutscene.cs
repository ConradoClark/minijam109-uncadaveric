using System;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Unity.Objects;

public class StartCutscene : BaseUIObject
{
    public Heart Heart;
    public ScriptTrigger SkipTutorial;
    private TextBox _textBox;
    private ChallengeHandler _challengeHandler;
    protected override void OnAwake()
    {
        base.OnAwake();
        _textBox = SceneObject<TextBox>.Instance();
        _challengeHandler = SceneObject<ChallengeHandler>.Instance();
    }

    private void OnEnable()
    {
        if (SkipTutorial.Triggered)
        {
            DefaultMachinery.AddBasicMachine(SkipCutscene());
        }
        else
        {
            DefaultMachinery.AddBasicMachine(ShowCutscene());
        }
    }

    private IEnumerable<IEnumerable<Action>> SkipCutscene()
    {
        Heart.Activate();
        yield return Heart.SkipTutorial().AsCoroutine();

        _challengeHandler.Activate();
    }

    private IEnumerable<IEnumerable<Action>> ShowCutscene()
    {
        yield return _textBox.ShowText("Have you ever wondered what happens when you kick your boots?").AsCoroutine();
        yield return _textBox.ShowText("After rigor mortis sets in").AsCoroutine();
        yield return _textBox.ShowText("And your body is sealed beneath the ground?").AsCoroutine();
        yield return _textBox.ShowText("After your pitiless soul falls into the unknown?").AsCoroutine();
        yield return _textBox.ShowText("Well...").AsCoroutine();
        // Character falls into the ground
        yield return _textBox.ShowText("You forget.").AsCoroutine();
        yield return _textBox.ShowText("Rather, you realize. You never knew to begin with.").AsCoroutine();
        yield return _textBox.ShowText("Cast into the depths of the purgatory, you notice...").AsCoroutine();
        yield return _textBox.ShowText("It was your body, unlike you, that pulled all the strings.").AsCoroutine();
        yield return _textBox.ShowText("It is time. Remember! Alas, learn!").AsCoroutine();
        
        Heart.Activate();
        yield return Heart.PlayTutorial().AsCoroutine();

        _challengeHandler.Activate();
    }
}
