using System;
using System.Collections.Generic;
using System.Linq;
using Licht.Impl.Generation;
using Licht.Impl.Orchestration;
using Licht.Interfaces.Generation;
using Licht.Unity.Extensions;
using Licht.Unity.Objects;
using UnityEngine;
using Random = UnityEngine.Random;

public class Shop : BaseUIObject, IGenerator<int, float>
{
    public SpriteRenderer ShopSpriteRenderer;

    public Vector3 SpawnPoint1;
    public Vector3 SpawnPoint2;
    public Vector3 SpawnPoint3;
    public Vector3 SpawnPoint4;

    public Vector3 TargetPoint1;
    public Vector3 TargetPoint2;
    public Vector3 TargetPoint3;
    public Vector3 TargetPoint4;

    public ShopItem[] ShopItems;

    private List<ShopItem> _selectedItems;
    private Heart _heart;
    private LevelManager _levelManager;

    public bool Open { get; private set; }
    protected override void OnAwake()
    {
        base.OnAwake();
        _selectedItems = new List<ShopItem>();
        _heart = SceneObject<Heart>.Instance();
        _levelManager = SceneObject<LevelManager>.Instance();
    }

    public IEnumerable<IEnumerable<Action>> SpawnShop()
    {
        Open = true;
        yield return SpawnItems().AsCoroutine();

        yield return _heart.EnterShopMode(() => Open, () => DefaultMachinery.AddBasicMachine(HideItems())).AsCoroutine();
    }

    public void CloseShop()
    {
        Open = false;
    }

    public IEnumerable<IEnumerable<Action>> ShuffleShop()
    {
        if (!Open)
            yield break;
        yield return HideItemsAndWait().AsCoroutine();
        yield return SpawnItems().AsCoroutine();
    }

    private IEnumerable<IEnumerable<Action>> SpawnItems()
    {
        _selectedItems.Clear();

        var selectableItems =
            ShopItems.Where(i => _levelManager.Level >= i.MinLevel && _levelManager.Level <= i.MaxLevel).ToArray();

        var spawnPoints = new[] { SpawnPoint1, SpawnPoint2, SpawnPoint3, SpawnPoint4 };
        var targetPoints = new[] { TargetPoint1, TargetPoint2, TargetPoint3, TargetPoint4 };

        var rng = new WeightedDice<ShopItem>(selectableItems, this, false);

        for (var i = 0; i < 4; i++)
        {
            var shopItem = rng.Generate();
            if (shopItem == null) break;

            shopItem.Reset();
            shopItem.transform.position = spawnPoints[i];

            DefaultMachinery.AddBasicMachine(shopItem.transform.GetAccessor()
                .Position
                .Y
                .SetTarget(targetPoints[i].y)
                .Over(1f)
                .Easing(EasingYields.EasingFunction.QuadraticEaseInOut)
                .UsingTimer(UITimer)
                .Build());

            _selectedItems.Add(shopItem);
            yield return TimeYields.WaitMilliseconds(UITimer, 200);
        }
    }

    private IEnumerable<IEnumerable<Action>> HideItems()
    {
        var points = new [] { SpawnPoint1, SpawnPoint2, SpawnPoint3, SpawnPoint4 };
        for (var i=0; i < _selectedItems.Count;i++)
        {
            var shopItem = _selectedItems[i];
            DefaultMachinery.AddBasicMachine(shopItem.transform.GetAccessor()
                .Position
                .Y
                .SetTarget(points[i].y)
                .Over(1f)
                .Easing(EasingYields.EasingFunction.QuadraticEaseInOut)
                .UsingTimer(UITimer)
                .Build());

            yield return TimeYields.WaitMilliseconds(UITimer, 100);
        }
    }

    private IEnumerable<IEnumerable<Action>> HideItemsAndWait()
    {
        var points = new[] { SpawnPoint1, SpawnPoint2, SpawnPoint3, SpawnPoint4 };
        for (var i = 0; i < _selectedItems.Count; i++)
        {
            var shopItem = _selectedItems[i];
            yield return shopItem.transform.GetAccessor()
                .Position
                .Y
                .SetTarget(points[i].y)
                .Over(.4f)
                .Easing(EasingYields.EasingFunction.QuadraticEaseInOut)
                .UsingTimer(UITimer)
                .Build();
        }
    }

    public int Seed { get; set; }
    public float Generate()
    {
        return Random.value;
    }
}
