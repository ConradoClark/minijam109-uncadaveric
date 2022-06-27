namespace Assets.Scripts.ShopItems
{
    public class Defibrillator : ShopItemFunction
    {
        public ItemCounter DefibrillatorCounter;
        public int Amount;

        public override void Execute()
        {
            DefibrillatorCounter.GiveItem(Amount);
        }
    }
}
