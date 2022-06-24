using System;
using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Unity.Objects;
using UnityEngine;

public class StartCutscene : BaseUIObject
{
    private TextBox _textBox;
    protected override void OnAwake()
    {
        base.OnAwake();
        _textBox = SceneObject<TextBox>.Instance();
    }

    private void OnEnable()
    {
        DefaultMachinery.AddBasicMachine(ShowCutscene());
    }

    private IEnumerable<IEnumerable<Action>> ShowCutscene()
    {
        yield return _textBox.ShowText("Hello!").AsCoroutine();
        yield return _textBox.ShowText("I'm testing the TextBox component...").AsCoroutine();
        yield return _textBox.ShowText("Let's do this baby!").AsCoroutine();
    }
}
