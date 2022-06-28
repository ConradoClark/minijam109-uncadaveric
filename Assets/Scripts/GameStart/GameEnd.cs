using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Licht.Impl.Orchestration;
using Licht.Unity.Objects;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameEnd : BaseUIObject
{
    public TMP_Text Level;
    public TMP_Text SurvivedFor;

    private LevelManager _levelManager;
    private PlayerInput _playerInput;

    protected override void OnAwake()
    {
        base.OnAwake();
        _levelManager = SceneObject<LevelManager>.Instance();
        _playerInput = PlayerInput.GetPlayerByIndex(0);
    }

    private void OnEnable()
    {
        Level.text = $"You got to Purgatory Level {_levelManager.Level}";

        var minutes = Mathf.FloorToInt(Time.timeSinceLevelLoad / 60f);
        var seconds = Mathf.FloorToInt(Time.timeSinceLevelLoad) % 60;

        SurvivedFor.text = $"You survived for {minutes} minutes and {seconds} seconds.";

        DefaultMachinery.AddBasicMachine(AnyKeyToRestart());
    }

    private IEnumerable<IEnumerable<Action>> AnyKeyToRestart()
    {
        yield return TimeYields.WaitSeconds(UITimer, 1);

        while (isActiveAndEnabled)
        {
            if (!_playerInput.actions.Any(act => act.type == InputActionType.Button && act.WasPerformedThisFrame()))
            {
                yield return TimeYields.WaitOneFrameX;
                continue;
            }

            DefaultMachinery.FinalizeWith(() =>
            {
                SceneManager.LoadScene("Scenes/Game");
            });
            break;
        }
    }
}
