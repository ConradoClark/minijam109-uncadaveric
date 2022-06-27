using Licht.Unity.Objects;

namespace Assets.Scripts.ShopItems
{
    public class ItemHeal : ShopItemFunction
    {
        public int Amount;
        private LifeBar _lifeBar;
        protected override void OnAwake()
        {
            base.OnAwake();
            _lifeBar = SceneObject<LifeBar>.Instance();
        }

        public override void Execute()
        {
            _lifeBar.Heal(Amount);
        }
    }
}
