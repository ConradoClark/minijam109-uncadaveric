using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Licht.Impl.Orchestration;
using Licht.Unity.Objects;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeartLaserParticle : BaseGameObject
{
    public float ParticleFrequencyInMs;
    public Collider2D LaserBounds;
    public ScriptPrefab LaserPrefab;

    public ScriptInput MousePos;
    public ScriptInput MouseClick;

    private PlayerInput _playerInput;
    private Camera _camera;
    private InputAction _mousePos;
    private InputAction _mouseClick;

    private ChallengeHandler _challengeHandler;
    private Shop _shop;

    protected override void OnAwake()
    {
        base.OnAwake();
        _challengeHandler = SceneObject<ChallengeHandler>.Instance();
        _shop = SceneObject<Shop>.Instance();
        _camera = SceneObject<UICamera>.Instance().Camera;
        _playerInput = PlayerInput.GetPlayerByIndex(0);
        _mousePos = _playerInput.actions[MousePos.ActionName];
        _mouseClick = _playerInput.actions[MouseClick.ActionName];
    }

    private void OnEnable()
    {
        DefaultMachinery.AddBasicMachine(HandleParticles());
    }

    private IEnumerable<IEnumerable<Action>> HandleParticles()
    {
        while (isActiveAndEnabled)
        {
            var mousePos = _mousePos.ReadValue<Vector2>();
            var pos = _camera.ScreenToWorldPoint(mousePos);
            if (!_shop.Open && _challengeHandler.IsActive && LaserBounds.OverlapPoint(pos) && _mouseClick.IsPressed())
            {
                if (LaserPrefab.Pool.TryGetFromPool(out var effect))
                {
                    effect.Component.transform.position = new Vector3(pos.x,pos.y,0);
                }
                yield return TimeYields.WaitMilliseconds(GameTimer, ParticleFrequencyInMs);
            }
            else yield return TimeYields.WaitOneFrameX;
        }
    }
}

