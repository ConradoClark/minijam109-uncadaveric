using System;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Unity.Extensions;
using Licht.Unity.Objects;
using Licht.Unity.Pooling;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Virus : EffectPoolable
{
    public int Points;
    public float Speed;
    public int HitPoints;
    public float HitCooldownInMs;
    public bool ActivateOnAwake;
    public SpriteRenderer SpriteRenderer;

    public Collider2D Collider;
    public ScriptInput MousePos;
    public ScriptInput MouseClick;

    public AudioSource HitSound;
    public ScriptPrefab DeathEffect;

    private PlayerInput _playerInput;
    private Camera _camera;
    private InputAction _mousePos;
    private InputAction _mouseClick;
    private int _currentHitPoints;
    private ColorDefaults _colorDefaults;
    private Heart _heart;
    private Points _points;

    private bool _updatingColor;

    protected override void OnAwake()
    {
        base.OnAwake();

        _camera = SceneObject<UICamera>.Instance().Camera;
        _playerInput = PlayerInput.GetPlayerByIndex(0);
        _mousePos = _playerInput.actions[MousePos.ActionName];
        _mouseClick = _playerInput.actions[MouseClick.ActionName];
        _colorDefaults = SceneObject<ColorDefaults>.Instance();
        _heart = SceneObject<Heart>.Instance();
        _points = SceneObject<Points>.Instance();

        if (ActivateOnAwake)
        {
            OnActivation();
        }
    }

    private void UpdateColor()
    {
        if (_updatingColor)
        {
            return;
        }

        _updatingColor = true;
        DefaultMachinery.AddBasicMachine(Flash());
    }

    private IEnumerable<IEnumerable<Action>> Flash()
    {
        var color = new Color(_colorDefaults.Monochrome.Color.r, _colorDefaults.Monochrome.Color.g,
            _colorDefaults.Monochrome.Color.b, 1 - (float)_currentHitPoints / HitPoints);

        yield return SpriteRenderer.GetAccessor()
            .Material("_Colorize")
            .AsColor()
            .ToColor(Color.HSVToRGB(Random.value,1,1))
            .Over(0.05f)
            .Easing(EasingYields.EasingFunction.CubicEaseOut)
            .UsingTimer(GameTimer)
            .Build();

        yield return SpriteRenderer.GetAccessor()
            .Material("_Colorize")
            .AsColor()
            .ToColor(color)
            .Over(0.05f)
            .Easing(EasingYields.EasingFunction.CubicEaseIn)
            .UsingTimer(GameTimer)
            .Build();

        _updatingColor = false;
    }

    private IEnumerable<IEnumerable<Action>> HandleMouse()
    {
        while (isActiveAndEnabled)
        {
            var mousePos = _mousePos.ReadValue<Vector2>();
            var pos = _camera.ScreenToWorldPoint(mousePos);
            if (Collider.OverlapPoint(pos) && _mouseClick.IsPressed())
            {
                _currentHitPoints--;
                UpdateColor();

                if (HitSound != null)
                {
                    HitSound.pitch = 0.5f + Random.value;
                    HitSound.Play();
                }
                yield return TimeYields.WaitMilliseconds(GameTimer, HitCooldownInMs);
            }

            if (_currentHitPoints <= 0)
            {
                _points.AddPoints(Points);
                if (DeathEffect.Pool.TryGetFromPool(out var effect))
                {
                    effect.Component.transform.position = transform.position;
                }
                IsEffectOver = true;
                if (ActivateOnAwake)
                {
                    gameObject.SetActive(false);
                }
            }

            yield return TimeYields.WaitOneFrameX;
        }
    }

    private IEnumerable<IEnumerable<Action>> GoTowardsHeart()
    {
        while (isActiveAndEnabled)
        {
            var target = Vector2.MoveTowards(transform.position, _heart.transform.position,
                Speed * (float)GameTimer.UpdatedTimeInMilliseconds * 0.001f);

            transform.position = new Vector3(target.x, target.y, 0);

            if (Vector2.Distance(transform.position, _heart.transform.position) < 0.1f)
            {
                if (DeathEffect.Pool.TryGetFromPool(out var effect))
                {
                    effect.Component.transform.position = transform.position;
                }

                IsEffectOver = true;
                _heart.InstantFlatline();
                break;
            }

            yield return TimeYields.WaitOneFrameX;
        }
    }

    private IEnumerable<IEnumerable<Action>> Wander()
    {
        while (isActiveAndEnabled)
        {
            if (SpriteRenderer == null)
            {
                yield return TimeYields.WaitOneFrameX;
                continue;
            }

            yield return SpriteRenderer.transform.GetAccessor()
                .LocalPosition
                .Y
                .Increase(0.05f)
                .Over(0.5f)
                .Easing(EasingYields.EasingFunction.QuadraticEaseInOut)
                .UsingTimer(GameTimer)
                .Build();

            yield return SpriteRenderer.transform.GetAccessor()
                .LocalPosition
                .Y
                .Decrease(0.05f)
                .Over(0.5f)
                .Easing(EasingYields.EasingFunction.QuadraticEaseInOut)
                .UsingTimer(GameTimer)
                .Build();
        }
    }

    public override void OnActivation()
    {
        _currentHitPoints = HitPoints;
        UpdateColor();
        DefaultMachinery.AddBasicMachine(Wander());
        DefaultMachinery.AddBasicMachine(GoTowardsHeart());
        DefaultMachinery.AddBasicMachine(HandleMouse());
    }

    public override bool IsEffectOver { get; protected set; }
}
