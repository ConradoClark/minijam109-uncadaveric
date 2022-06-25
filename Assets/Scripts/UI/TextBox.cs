using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Licht.Impl.Orchestration;
using Licht.Unity.Objects;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TextBox : BaseUIObject
{
    public float LetterFrequencyInMs;
    public SpriteRenderer CursorSprite;
    public TMP_Text TextComponent;
    public ScriptInput ConfirmButton;
    public bool Skippable;

    private PlayerInput _playerInput;
    protected override void OnAwake()
    {
        base.OnAwake();
        _playerInput = PlayerInput.GetPlayerByIndex(0);
    }


    public IEnumerable<IEnumerable<Action>> ShowText(string text, bool showCursor = true)
    {
        CursorSprite.enabled = showCursor;
        TextComponent.text = "";

        var confirm = _playerInput.actions[ConfirmButton.ActionName];

        var i = 0;
        while (TextComponent.text != text)
        {
            TextComponent.text = text[..i];
            i++;
            yield return TimeYields.WaitMilliseconds(UITimer, LetterFrequencyInMs,
                breakCondition: () => showCursor && Skippable && confirm.WasPerformedThisFrame());

            // Skip
            if (showCursor && Skippable && confirm.WasPerformedThisFrame())
            {
                TextComponent.text = text;
            }
        }

        if (showCursor)
        {
            CursorSprite.enabled = true;
            yield return TimeYields.WaitOneFrameX;

            while (!confirm.WasPerformedThisFrame())
            {
                yield return TimeYields.WaitOneFrameX;
            }   

            TextComponent.text = "";
            CursorSprite.enabled = false;
        }

        yield return TimeYields.WaitOneFrameX;
    }
}
