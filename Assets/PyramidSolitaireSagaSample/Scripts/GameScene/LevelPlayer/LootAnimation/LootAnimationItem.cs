using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.LootAnimation
{
    public class LootAnimationItem
    {
        public Sprite CardSprite { get; private set; }
        public long BonusCount { get; private set; }
        public Vector3 StartPosition { get; private set; }
        public Quaternion StartRotation { get; private set; }
        public ILootAnimationEndPoint LootAnimationEndPoint { get; private set; }
        public Vector3 EndPosition { get; private set; }
        public Action OnComplete { get; private set; }

        internal void Init(
            long bonusCount,
            Sprite cardSprite, 
            Vector3 startPosition, 
            Quaternion startRotation, 
            ILootAnimationEndPoint lootAnimationEndPoint,
            Action onComplete
        )
        {
            CardSprite = cardSprite;
            BonusCount = bonusCount;
            StartPosition = startPosition;
            StartRotation = startRotation;
            LootAnimationEndPoint = lootAnimationEndPoint;
            EndPosition = LootAnimationEndPoint.GetLootAnimtionEndPoint();
            OnComplete = onComplete;
        }
    }
}
