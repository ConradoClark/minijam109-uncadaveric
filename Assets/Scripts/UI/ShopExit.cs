using System;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Unity.Objects;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopExit : BaseUIObject
{
    public SpriteRenderer Border;
    public SpriteRenderer BG;
    public TMP_Text ExitText;

    public Collider2D Collider;

    public ScriptInput MousePos;
    public ScriptInput MouseClick;


    private PlayerInput _playerInput;
    private ColorDefaults _colorDefaults;
    private Shop _shop;
    private TextBox _textBox;

    private InputAction _mousePos;
    private InputAction _mouseClick;

    public Camera Camera;

    protected override void OnAwake()
    {
        base.OnAwake();
        _colorDefaults = SceneObject<ColorDefaults>.Instance();
        _shop = SceneObject<Shop>.Instance();
        _playerInput = PlayerInput.GetPlayerByIndex(0);
        _textBox = SceneObject<TextBox>.Instance();
        _mousePos = _playerInput.actions[MousePos.ActionName];
        _mouseClick = _playerInput.actions[MouseClick.ActionName];
    }

    private void OnEnable()
    {
        DefaultMachinery.AddBasicMachine(HandleHover());
        DefaultMachinery.AddBasicMachine(HandleExitButton());
    }

    private IEnumerable<IEnumerable<Action>> HandleHover()
    {
        while (isActiveAndEnabled)
        {
            mainloop:
            BG.material.SetColor("_Colorize", _colorDefaults.Dark.Color);

            while (!_shop.Open)
            {
                yield return TimeYields.WaitOneFrameX;
            }

            var mousePos = _mousePos.ReadValue<Vector2>();
            var pos = Camera.ScreenToWorldPoint(mousePos);
            // is hovering
            if (Collider.OverlapPoint(pos))
            {
                BG.material.SetColor("_Colorize", _colorDefaults.Recharging.Color);
                DefaultMachinery.AddBasicMachine(_textBox.ShowText("Click to exit the shop.", false));
            }
            else
            {
                yield return TimeYields.WaitOneFrameX;
                continue;
            }

            while (Collider.OverlapPoint(pos) && _shop.Open)
            {
                mousePos = _mousePos.ReadValue<Vector2>();
                pos = Camera.ScreenToWorldPoint(mousePos);

                if (_mouseClick.WasPerformedThisFrame())
                {
                    _shop.CloseShop();
                    
                    goto mainloop;
                }

                yield return TimeYields.WaitOneFrameX;
            }

            yield return _textBox.ShowText("", false, true).AsCoroutine();
        }
    }

    private IEnumerable<IEnumerable<Action>> HandleExitButton()
    {
        while (isActiveAndEnabled)
        {
            if (_shop.Open)
            {
                Border.enabled = BG.enabled = ExitText.enabled = true;
            }

            while (_shop.Open)
            {
                yield return TimeYields.WaitOneFrameX;
            }

            Border.enabled = BG.enabled = ExitText.enabled = false;

            yield return TimeYields.WaitOneFrameX;
        }
    }

}

