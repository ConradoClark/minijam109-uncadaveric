using System;
using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Unity.Objects;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleScreen : BaseUIObject
{
    public Transform BlinkObject;
    public ScriptInput SpaceBar;
    public ScriptInput MouseClick;
    private PlayerInput _playerInput;
    protected override void OnAwake()
    {
        base.OnAwake();
        _playerInput = PlayerInput.GetPlayerByIndex(0);
        DefaultMachinery.AddBasicMachine(Blink());
        DefaultMachinery.AddBasicMachine(HandleTitle());
    }

    private IEnumerable<IEnumerable<Action>> Blink()
    {
        while (isActiveAndEnabled)
        {
            yield return TimeYields.WaitMilliseconds(UITimer, 200);
            BlinkObject.gameObject.SetActive(false);
            yield return TimeYields.WaitMilliseconds(UITimer, 200);
            BlinkObject.gameObject.SetActive(true);
        }
    }

    private IEnumerable<IEnumerable<Action>> HandleTitle()
    {
        var action1 = _playerInput.actions[SpaceBar.ActionName];
        var action2 = _playerInput.actions[MouseClick.ActionName];
        while (isActiveAndEnabled)
        {
            if (action1.WasPerformedThisFrame() || action2.WasPerformedThisFrame())
            {
                DefaultMachinery.FinalizeWith(() =>
                {
                    SceneManager.LoadScene("Scenes/Game");
                });
                break;
            }
            yield return TimeYields.WaitOneFrameX;
        }
    }
}
