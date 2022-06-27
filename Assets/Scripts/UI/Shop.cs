using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Licht.Impl.Events;
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
    public bool Open { get; private set; }
    protected override void OnAwake()
    {
        base.OnAwake();
        _selectedItems = new List<ShopItem>();
        _heart = SceneObject<Heart>.Instance();
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

    // every 2 shops, there MUST be a healing item.
    // other than that, everything may be random.

    private IEnumerable<IEnumerable<Action>> SpawnItems()
    {
        _selectedItems.Clear();

        var rng = new WeightedDice<ShopItem>(ShopItems, this);
        // prevent item repeat
        // if no items left, skip
        var shopItem1 = rng.Generate();

        shopItem1.Reset();
        shopItem1.transform.position = SpawnPoint1;

        yield return shopItem1.transform.GetAccessor()
            .Position
            .Y
            .SetTarget(TargetPoint1.y)
            .Over(1f)
            .Easing(EasingYields.EasingFunction.QuadraticEaseInOut)
            .UsingTimer(UITimer)
            .Build();

        _selectedItems.Add(shopItem1);  
        //var shopItem2 = rng.Generate();
        //var shopItem3 = rng.Generate();
        //var shopItem4 = rng.Generate();
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

    public int Seed { get; set; }
    public float Generate()
    {
        return Random.value;
    }
}
