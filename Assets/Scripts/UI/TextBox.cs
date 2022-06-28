using System;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Unity.Objects;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TextBox : BaseUIObject
{
    public float LetterFrequencyInMs;
    public SpriteRenderer CursorSprite;
    public SpriteRenderer CursorHelpSprite;
    public TMP_Text TextComponent;
    public ScriptInput ConfirmButton;
    public bool Skippable;

    private PlayerInput _playerInput;

    private bool _showingText;
    private string _currentText;
    protected override void OnAwake()
    {
        base.OnAwake();
        _playerInput = PlayerInput.GetPlayerByIndex(0);
    }


    public IEnumerable<IEnumerable<Action>> ShowText(string text, bool showCursor = true, bool overlap = false)
    {
        while (_showingText)
        {
            if (_currentText == text) yield break;
            if (overlap) _showingText = false;

            yield return TimeYields.WaitOneFrameX;
        }

        _showingText = true;
        _currentText = text;
        CursorSprite.enabled = CursorHelpSprite.enabled = showCursor;
        TextComponent.text = "";

        var confirm = _playerInput.actions[ConfirmButton.ActionName];

        var i = 0;
        while (TextComponent.text != text)
        {
            TextComponent.text = text[..i];
            i++;
            yield return TimeYields.WaitMilliseconds(UITimer, LetterFrequencyInMs,
                breakCondition: () =>!_showingText || showCursor && Skippable && confirm.WasPerformedThisFrame());

            // Skip
            if (showCursor && Skippable && confirm.WasPerformedThisFrame())
            {
                TextComponent.text = text;
            }
        }

        if (showCursor && _showingText)
        {
            CursorSprite.enabled = CursorHelpSprite.enabled = true;
            yield return TimeYields.WaitOneFrameX;

            while (!confirm.WasPerformedThisFrame())
            {
                yield return TimeYields.WaitOneFrameX;
            }   

            TextComponent.text = "";
            CursorSprite.enabled = CursorHelpSprite.enabled = false;
        }

        yield return TimeYields.WaitOneFrameX;
        _showingText = false;
    }
}
