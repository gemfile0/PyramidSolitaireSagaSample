using DG.Tweening;
using PyramidSolitaireSagaSample.GameCommon.UI;
using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.Helper;
using PyramidSolitaireSagaSample.System;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace PyramidSolitaireSagaSample.LevelPlayer.LootAnimation
{
    public interface ILootAnimationEndPoint
    {
        Vector3 GetLootAnimtionEndPoint();
        void BeginLootAnimation();
        void EndLootAnimation(long bonusCount);
    }

    public interface ILootAnimationTrigger
    {
        event Action<LootAnimationType, LootAnimationInfo> requestLootAnimation;
    }

    public class LootAnimationInfo
    {
        public Sprite CardSprite { get; }
        public long BonusCount { get; }
        public Vector3 StartPosition { get; }
        public Quaternion StartRotation { get; }
        public Action OnItemComplete { get; }

        public LootAnimationInfo(long bonusCount, Sprite cardSprite, Vector3 startPosition, Quaternion startRotation, Action onItemComplete)
        {
            CardSprite = cardSprite;
            BonusCount = bonusCount;
            StartPosition = startPosition;
            StartRotation = startRotation;
            OnItemComplete = onItemComplete;
        }
    }

    public enum LootAnimationType
    {
        QuestionCard,
        JokerCard,
        Coin
    }

    public class LootAnimationController : MonoBehaviour, IGameObjectFinderSetter
    {
        [Header("Data")]
        [SerializeField] private LootAnimationData _lootAnimationData;

        [Header("View")]
        [SerializeField] private SpriteRendererContainer _cardRendererPrefab;
        [SerializeField] private SpriteRendererContainer _coinRendererPrefab;
        [SerializeField] private Transform _itemRendererRoot;
        [SerializeField] private TextContainer _countTextPrefab;

        private IEnumerable<ILootAnimationTrigger> _lootAnimationTriggers;

        private GameObjectPool<SpriteRendererContainer> _cardRendererPool;
        private GameObjectPool<SpriteRendererContainer> _coinRendererPool;
        private Stack<List<SpriteRendererContainer>> _coinRendererListPool;
        private GameObjectPool<TextContainer> _countTextPool;
        private Dictionary<LootAnimationType, ILootAnimationEndPoint> _endPointDict;
        private ObjectPool<LootAnimationItem> _lootAnimationItemPool;

        private int _itemIndex;

        public void OnGameObjectFinderAwake(IGameObjectFinder finder)
        {
            _lootAnimationTriggers = finder.FindGameObjectOfType<ILootAnimationTrigger>();
        }

        private void Awake()
        {
            _cardRendererPool = new(_itemRendererRoot, _cardRendererPrefab.gameObject, defaultCapacity: 2);
            _countTextPool = new(_itemRendererRoot, _countTextPrefab.gameObject, defaultCapacity: 2);
            _coinRendererPool = new(_itemRendererRoot, _coinRendererPrefab.gameObject, defaultCapacity: 10);
            _coinRendererListPool = new();
            _endPointDict = new();
            _lootAnimationItemPool = new(createFunc: () => new LootAnimationItem(),
                                         defaultCapacity: 2);
            _itemIndex = 0;
        }

        private void OnEnable()
        {
            foreach (ILootAnimationTrigger trigger in _lootAnimationTriggers)
            {
                trigger.requestLootAnimation += PlayLootAnimation;
            }
        }

        private void OnDisable()
        {
            foreach (ILootAnimationTrigger trigger in _lootAnimationTriggers)
            {
                trigger.requestLootAnimation += PlayLootAnimation;
            }
        }

        public void AddLootAnimationEndPoint(LootAnimationType type, ILootAnimationEndPoint endPoint)
        {
            _endPointDict.Add(type, endPoint);
        }

        private void PlayLootAnimation(LootAnimationType type, LootAnimationInfo info)
        {
            LootAnimationItem item = _lootAnimationItemPool.Get();
            item.Init(info.BonusCount,
                      info.CardSprite,
                      info.StartPosition,
                      info.StartRotation,
                      _endPointDict[type],
                      info.OnItemComplete);

            switch (type)
            {
                case LootAnimationType.QuestionCard:
                case LootAnimationType.JokerCard:
                    AnimateCard(item);
                    break;

                case LootAnimationType.Coin:
                    AnimateCoin(item);
                    break;
            }
        }

        private void AnimateCoin(LootAnimationItem item)
        {
            long coinCount = (item.BonusCount - 1) / 10 + 1;

            List<SpriteRendererContainer> coinRendererList = GetCoinRendererList();
            for (int i = 0; i < coinCount; i++)
            {
                Vector3 randomOffset = UnityEngine.Random.insideUnitSphere;
                SpriteRendererContainer coinRenderer = GetSpriteRendererContainer(
                    _coinRendererPool,
                    item.StartPosition,
                    item.StartRotation,
                    randomOffset
                );
                coinRenderer.Init(i, null);
                coinRendererList.Add(coinRenderer);
                IncreaseItemIndex();
            }

            TextContainer countText = GetCountText(new Vector3(item.EndPosition.x, item.EndPosition.y + _lootAnimationData.TextStartOffsetY, item.EndPosition.z));
            IncreaseItemIndex();

            // 연출 시작
            item.LootAnimationEndPoint.BeginLootAnimation();

            Sequence totalSequence = DOTween.Sequence();
            float spawnDelay = _lootAnimationData.SpawnDelay;
            float sequenceDelay = 0f;
            for (int i = 0; i < coinCount; i++)
            {
                SpriteRendererContainer coinRenderer = coinRendererList[i];

                sequenceDelay = spawnDelay * i;
                Sequence sequence = DOTween.Sequence();
                sequence.Append(coinRenderer.SpriteRenderer.DOFade(1f, _lootAnimationData.FadeDuration)
                                                           .SetEase(_lootAnimationData.FadeEase));
                sequence.Join(coinRenderer.CachedTransform.DORotate(Vector3.up, _lootAnimationData.MoveDuration)
                                                          .SetEase(_lootAnimationData.MoveEase));

                Vector3 startPosition = coinRenderer.CachedTransform.position;
                Vector3 diff = item.EndPosition - startPosition;
                Vector3 midPoint = startPosition + diff / 2 + Vector3.up * _lootAnimationData.JumpHeight;
                Vector3[] path = new Vector3[] { startPosition, midPoint, item.EndPosition };
                sequence.Join(coinRenderer.CachedTransform.DOPath(path, _lootAnimationData.MoveDuration, _lootAnimationData.PathType)
                                                          .SetEase(_lootAnimationData.MoveEase));
                sequence.Append(coinRenderer.SpriteRenderer.DOFade(0f, _lootAnimationData.FadeDuration)
                                                           .SetEase(_lootAnimationData.FadeEase));

                totalSequence.Insert(sequenceDelay, sequence);
            }

            float moveDuration = sequenceDelay + _lootAnimationData.MoveDuration;
            totalSequence.InsertCallback(moveDuration, () => countText.UpdateText($"+{item.BonusCount}"));
            totalSequence.Join(countText.CachedTransform.DOMoveY(item.EndPosition.y + _lootAnimationData.TextEndOffsetY, _lootAnimationData.TextDuration)
                                                        .SetEase(_lootAnimationData.TextEase));
            totalSequence.Append(countText.TextMeshPro.DOFade(0f, _lootAnimationData.FadeDuration)
                                                      .SetEase(_lootAnimationData.FadeEase));
            // 코인 연출은 텍스트 사라지는 타이밍에 완료
            totalSequence.JoinCallback(() =>
            {
                item.LootAnimationEndPoint.EndLootAnimation(item.BonusCount);
                item.OnComplete?.Invoke();
            });
            totalSequence.OnComplete(() =>
            {
                for (int i = 0; i < coinCount; i++)
                {
                    SpriteRendererContainer coinRenderer = coinRendererList[i];
                    _coinRendererPool.Release(coinRenderer);
                }
                coinRendererList.Clear();
                ReleaseCoinRendererList(coinRendererList);

                _countTextPool.Release(countText);
                _lootAnimationItemPool.Release(item);
            });
        }

        private void ReleaseCoinRendererList(List<SpriteRendererContainer> coinRendererList)
        {
            _coinRendererListPool.Push(coinRendererList);
        }

        private List<SpriteRendererContainer> GetCoinRendererList()
        {
            if (_coinRendererListPool.Count == 0)
            {
                _coinRendererListPool.Push(new List<SpriteRendererContainer>());
            }
            return _coinRendererListPool.Pop();
        }

        private void AnimateCard(LootAnimationItem item)
        {
            SpriteRendererContainer cardRenderer = GetSpriteRendererContainer(
                _cardRendererPool,
                item.StartPosition,
                item.StartRotation
            );
            cardRenderer.Init(_itemIndex, null);
            cardRenderer.UpdateSpriteRenderer(item.CardSprite);
            IncreaseItemIndex();

            TextContainer countText = GetCountText(new Vector3(item.EndPosition.x, item.EndPosition.y + _lootAnimationData.TextStartOffsetY, item.EndPosition.z));
            IncreaseItemIndex();

            // 연출 시작
            item.LootAnimationEndPoint.BeginLootAnimation();

            Sequence sequence = DOTween.Sequence();
            sequence.Append(cardRenderer.SpriteRenderer.DOFade(1f, _lootAnimationData.FadeDuration)
                                                       .SetEase(_lootAnimationData.FadeEase));
            sequence.Insert(0f, cardRenderer.CachedTransform.DORotate(Vector3.up, _lootAnimationData.MoveDuration)
                                                            .SetEase(_lootAnimationData.MoveEase));

            Vector3 diff = item.EndPosition - item.StartPosition;
            Vector3 midPoint = item.StartPosition + diff / 2 + Vector3.up * _lootAnimationData.JumpHeight;
            Vector3[] path = new Vector3[] { item.StartPosition, midPoint, item.EndPosition };
            sequence.Insert(0, cardRenderer.CachedTransform.DOPath(path, _lootAnimationData.MoveDuration, _lootAnimationData.PathType)
                                                           .SetEase(_lootAnimationData.MoveEase));
            sequence.Append(cardRenderer.SpriteRenderer.DOFade(0f, _lootAnimationData.FadeDuration)
                                                       .SetEase(_lootAnimationData.FadeEase));

            sequence.JoinCallback(() => countText.TextMeshPro.text = $"+{item.BonusCount}");
            sequence.Join(countText.CachedTransform.DOMoveY(item.EndPosition.y + _lootAnimationData.TextEndOffsetY, _lootAnimationData.TextDuration)
                                                   .SetEase(_lootAnimationData.TextEase));
            // 카드 연출은 텍스트 나타나는 타이밍에 완료
            sequence.JoinCallback(() =>
            {
                item.LootAnimationEndPoint.EndLootAnimation(item.BonusCount);
                item.OnComplete?.Invoke();
            });
            sequence.Append(countText.TextMeshPro.DOFade(0f, _lootAnimationData.FadeDuration)
                                                 .SetEase(_lootAnimationData.FadeEase));
            sequence.OnComplete(() =>
            {
                _cardRendererPool.Release(cardRenderer);

                _countTextPool.Release(countText);
                _lootAnimationItemPool.Release(item);
            });
        }

        private void IncreaseItemIndex()
        {
            _itemIndex += 1;
            if (_itemIndex == int.MaxValue)
            {
                _itemIndex = 0;
            }
        }

        private SpriteRendererContainer GetSpriteRendererContainer(
            GameObjectPool<SpriteRendererContainer> itemPool,
            Vector3 startPosition,
            Quaternion startRotation,
            Vector3 startPositionOffset = default
        )
        {
            SpriteRendererContainer itemRenderer = itemPool.Get();
            itemRenderer.UpdateSortingLayer("Loot");
            itemRenderer.UpdateSortingOrder(_itemIndex);
            if (itemRenderer.CollierObject != null)
            {
                itemRenderer.CollierObject.SetActive(false);
            }
            itemRenderer.CachedTransform.SetParent(_itemRendererRoot);
            itemRenderer.CachedTransform.position = startPosition + startPositionOffset;
            itemRenderer.CachedTransform.rotation = startRotation;
            Color originRendererColor = itemRenderer.SpriteRenderer.color;
            itemRenderer.SpriteRenderer.color = new Color(originRendererColor.r, originRendererColor.g, originRendererColor.b, 0f);
            return itemRenderer;
        }

        private TextContainer GetCountText(Vector3 endPosition)
        {
            TextContainer countText = _countTextPool.Get();
            countText.TextMeshPro.text = "";
            countText.UpdateSortingLayer("Loot");
            countText.UpdateSortingOrder(_itemIndex);
            countText.CachedTransform.SetParent(_itemRendererRoot);
            countText.CachedTransform.position = endPosition;
            Color originTextColor = countText.TextMeshPro.color;
            countText.TextMeshPro.color = new Color(originTextColor.r, originTextColor.g, originTextColor.b, 1f);
            return countText;
        }

    }
}