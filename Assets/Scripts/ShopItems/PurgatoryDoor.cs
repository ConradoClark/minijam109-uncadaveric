using Licht.Unity.Objects;

public class PurgatoryDoor : ShopItemFunction
{
    private LevelManager _levelManager;

    protected override void OnAwake()
    {
        base.OnAwake();
        _levelManager = SceneObject<LevelManager>.Instance();
    }

    public override void Execute()
    {
        _levelManager.NextLevel();
    }
}
