using System;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Interfaces.Generation;
using Licht.Unity.Objects;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopItem : BaseUIObject, IWeighted<float>
{
    public ShopItemFunction Function;
    public int Cost;
    public SpriteRenderer BG;
    public Collider2D Collider;

    public ScriptInput MousePos;
    public ScriptInput MouseClick;

    private PlayerInput _playerInput;
    private ColorDefaults _colorDefaults;
    private Shop _shop;
    private TextBox _textBox;

    private InputAction _mousePos;
    private InputAction _mouseClick;

    public int ItemWeight;
    public float Weight => ItemWeight;

    public string ItemDescription;

    private Camera _camera;
    private Points _points;

    public bool Bought { get; private set; }

    public SpriteRenderer SoldOut;
    public TMP_Text SoldOutText;

    public TMP_Text CostText;

    protected override void OnAwake()
    {
        base.OnAwake();
        _colorDefaults = SceneObject<ColorDefaults>.Instance();
        _shop = SceneObject<Shop>.Instance();
        _playerInput = PlayerInput.GetPlayerByIndex(0);
        _textBox = SceneObject<TextBox>.Instance();
        _mousePos = _playerInput.actions[MousePos.ActionName];
        _mouseClick = _playerInput.actions[MouseClick.ActionName];
        _camera = SceneObject<UICamera>.Instance().Camera;
        _points = SceneObject<Points>.Instance();
        SoldOut.enabled = SoldOutText.enabled = false;
        CostText.text = Cost.ToString();
    }

    public void Reset()
    {
        Bought = false;
        SoldOut.enabled = SoldOutText.enabled = false;
    }

    private void OnEnable()
    {
        DefaultMachinery.AddBasicMachine(HandleHover());
    }

    private IEnumerable<IEnumerable<Action>> HandleHover()
    {
        while (isActiveAndEnabled)
        {
            mainloop:
            BG.material.SetColor("_Colorize", _colorDefaults.Dark.Color);

            while (!_shop.Open || Bought)
            {
                yield return TimeYields.WaitOneFrameX;
            }

            var mousePos = _mousePos.ReadValue<Vector2>();
            var pos = _camera.ScreenToWorldPoint(mousePos);
            // is hovering
            if (Collider.OverlapPoint(pos))
            {
                BG.material.SetColor("_Colorize", _colorDefaults.Recharging.Color);
                DefaultMachinery.AddBasicMachine(_textBox.ShowText(ItemDescription, false));
            }
            else
            {
                yield return TimeYields.WaitOneFrameX;
                continue;
            }

            while (Collider.OverlapPoint(pos) && _shop.Open && !Bought)
            {
                mousePos = _mousePos.ReadValue<Vector2>();
                pos = _camera.ScreenToWorldPoint(mousePos);

                if (_mouseClick.WasPerformedThisFrame() && _points.SpendPoints(Cost))
                {
                    Bought = true;
                    SoldOut.enabled = SoldOutText.enabled = true;
                    if (Function!=null) Function.Execute();
                    goto mainloop;
                }

                yield return TimeYields.WaitOneFrameX;
            }

            yield return _textBox.ShowText("", false, true).AsCoroutine();
        }
    }

}
