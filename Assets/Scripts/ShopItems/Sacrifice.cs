using Licht.Unity.Objects;

namespace Assets.Scripts.ShopItems
{
    public class Sacrifice : ShopItemFunction
    {
        private LifeBar _lifeBar;
        protected override void OnAwake()
        {
            base.OnAwake();
            _lifeBar = SceneObject<LifeBar>.Instance();
        }

        public override void Execute()
        {
            _lifeBar.MaximumLife /= 2;
            _lifeBar.Heal(_lifeBar.MaximumLife);
        }
    }
}
