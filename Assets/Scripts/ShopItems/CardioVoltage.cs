using Licht.Unity.Objects;

namespace Assets.Scripts.ShopItems
{
    public class CardioVoltage : ShopItemFunction
    {
        public int Amount;
        public ItemCounter DefibrillatorCounter;
        private LifeBar _lifeBar;
        protected override void OnAwake()
        {
            base.OnAwake();
            _lifeBar = SceneObject<LifeBar>.Instance();
        }

        public override void Execute()
        {
            var uses = 0;
            while (DefibrillatorCounter.UseItem())
            {
                uses++;
            }

            _lifeBar.Heal(uses * Amount);
        }
    }
}
