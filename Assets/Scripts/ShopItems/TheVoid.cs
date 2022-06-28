using Licht.Unity.Objects;

namespace Assets.Scripts.ShopItems
{
    public class TheVoid : ShopItemFunction
    {
        public int Amount;
        private LifeBar _lifeBar;
        protected override void OnAwake()
        {
            base.OnAwake();
            _lifeBar = SceneObject<LifeBar>.Instance(true);
        }

        public override void Execute()
        {
            _lifeBar.IncreaseMaximum(Amount);
        }
    }
}
