using PyramidSolitaireSagaSample.LevelPlayer.CardPool;
using PyramidSolitaireSagaSample.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardPool
{
    public class CardPoolEditorUI : MonoBehaviour
    {
        [SerializeField] private Transform _itemRoot;
        [FormerlySerializedAs("_itemPrefab")]
        [SerializeField] private CardPoolItemGroup _itemGroupPrefab;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private string _pressedTrigger = "Pressed";
        [SerializeField] private Animator _titleAnimator;

        public event Action<CardPoolItemModel, int> onValueChanged;
        public event Action<CardPoolItemModel, int> onButtonClick;

        private GameObjectPool<CardPoolItemGroup> _itemGroupPool;
        private Dictionary<CardNumber, CardPoolItemGroup> _itemGroupDict;
        private StringBuilder _itemTextBuilder;
        private string _latestTitleStr;

        public void Init()
        {
            _itemGroupPool = new GameObjectPool<CardPoolItemGroup>(parent: _itemRoot, prefab: _itemGroupPrefab.gameObject, maxSize: 30);
            _itemGroupDict = new Dictionary<CardNumber, CardPoolItemGroup>();
            _itemTextBuilder = new StringBuilder();
        }

        internal void UpdateUI(ReadOnlyDictionary<string, CardPoolItemModel> poolItemModelDict, ReadOnlyDictionary<string, CardPoolItemModel> totalItemModelDict)
        {
            int poolItemCount = 0;
            int totalItemCount = 0;
            foreach (KeyValuePair<string, CardPoolItemModel> pair in poolItemModelDict)
            {
                string key = pair.Key;
                CardPoolItemModel poolItemModel = pair.Value;
                CardPoolItemModel totalItemModel = totalItemModelDict[key];

                if (poolItemModel.cardNumber != CardNumber.Num_Random)
                {
                    CardPoolItemGroup itemGroup = GetItemGroup(poolItemModel);

                    string cardNumberStr = key.Substring(3);

                    _itemTextBuilder.Length = 0;
                    _itemTextBuilder.Append("  ");
                    _itemTextBuilder.Append(poolItemModel.count);
                    _itemTextBuilder.Append(" /");
                    string poolCountStr = _itemTextBuilder.ToString();
                    string totalCountStr = totalItemModel.count.ToString();

                    itemGroup.UpdateUI(totalItemModel,
                                       cardNumberStr,
                                       poolCountStr,
                                       totalCountStr,
                                       OnValueChanged,
                                       OnButtonClick);
                    if (poolItemModel.count <= 0)
                    {
                        itemGroup.DisablePoolCountUI(totalItemModel);
                    }

                    poolItemCount += poolItemModel.count;
                }

                totalItemCount += totalItemModel.count;
            }

            _itemTextBuilder.Length = 0;
            _itemTextBuilder.Append("Card Pool (");
            _itemTextBuilder.Append(poolItemCount);
            _itemTextBuilder.Append("/");
            _itemTextBuilder.Append(totalItemCount);
            _itemTextBuilder.Append(")");
            _latestTitleStr = _itemTextBuilder.ToString();
            if (_titleText.text != _latestTitleStr)
            {
                _titleText.text = _latestTitleStr;
                if (gameObject.activeInHierarchy)
                {
                    _titleAnimator.SetTrigger(_pressedTrigger);
                }
            }
        }

        private void OnValueChanged(CardPoolItemModel itemModel, int value)
        {
            onValueChanged?.Invoke(itemModel, value);
        }

        private void OnButtonClick(CardPoolItemModel itemModel, int valueOffset)
        {
            onButtonClick?.Invoke(itemModel, valueOffset);
        }

        private CardPoolItemGroup GetItemGroup(CardPoolItemModel itemModel)
        {
            if (_itemGroupDict.ContainsKey(itemModel.cardNumber) == false)
            {
                CardPoolItemGroup itemGroup = _itemGroupPool.Get();
                itemGroup.CachedRectTransform.SetParent(_itemRoot);
                itemGroup.CachedRectTransform.localScale = Vector3.one;
                _itemGroupDict[itemModel.cardNumber] = itemGroup;
            }
            return _itemGroupDict[itemModel.cardNumber];
        }

        internal void RevertItemUI(CardPoolItemModel itemModel)
        {
            CardPoolItemGroup itemGroup = GetItemGroup(itemModel);
            itemGroup.RevertUI(itemModel);
        }
    }
}
