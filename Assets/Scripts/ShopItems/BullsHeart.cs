﻿using Licht.Unity.Objects;

namespace Assets.Scripts.ShopItems
{
    public class BullsHeart : ShopItemFunction
    {
        private Heart _heart;
        protected override void OnAwake()
        {
            base.OnAwake();
            _heart = SceneObject<Heart>.Instance(true);
        }

        public override void Execute()
        {
            _heart.BpmTargetTolerance = 15;
        }
    }
}
