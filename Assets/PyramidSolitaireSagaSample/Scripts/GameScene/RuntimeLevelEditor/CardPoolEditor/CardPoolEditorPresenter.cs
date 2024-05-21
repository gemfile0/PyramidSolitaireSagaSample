using PyramidSolitaireSagaSample.LevelPlayer.CardDeck;
using PyramidSolitaireSagaSample.LevelPlayer.CardPool;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardPool
{
    public class CardPoolEditorPresenter : MonoBehaviour, 
                                           ILevelSavable
    {
        [Header("Model")]
        [SerializeField] private CardPoolModel _cardPoolModel;

        [Header("View")]
        [SerializeField] private CardPoolEditorUI _cardPoolUI;

        public event Action<ReadOnlyDictionary<string, CardPoolItemModel>, 
                            ReadOnlyDictionary<string, CardPoolItemModel>> onItemModelUpdated
        {
            add { _cardPoolModel.onItemModelUpdated += value; }
            remove { _cardPoolModel.onItemModelUpdated -= value; }
        }

        public event Action<int> onDeckCountUpdated
        {
            add { _cardPoolModel.onDeckCountUpdated += value; }
            remove { _cardPoolModel.onDeckCountUpdated -= value; }
        }

        public string RestoreLevelID => RestoreLevelIdPath.CardPool;

        private void Awake()
        {
            // 게임 오브젝트가 꺼져있기 때문에 수동으로 초기화
            _cardPoolUI.Init();
        }

        private void OnEnable()
        {
            _cardPoolModel.onItemModelUpdated += _cardPoolUI.UpdateUI;
            _cardPoolModel.onItemModelRestored += _cardPoolUI.UpdateUI;

            _cardPoolUI.onValueChanged += OnValueChanged;
            _cardPoolUI.onButtonClick += OnButtonClick;
        }

        private void OnDisable()
        {
            _cardPoolModel.onItemModelUpdated -= _cardPoolUI.UpdateUI;
            _cardPoolModel.onItemModelRestored -= _cardPoolUI.UpdateUI;

            _cardPoolUI.onValueChanged -= OnValueChanged;
            _cardPoolUI.onButtonClick -= OnButtonClick;
        }

        private void OnValueChanged(CardPoolItemModel itemModel, int nextCount)
        {
            if (nextCount >= 0 && nextCount <= 9)
            {
                _cardPoolModel.ChangeTotalCount(itemModel.cardNumber, itemModel.cardColor, nextCount);
            }
            else
            {
                _cardPoolUI.RevertItemUI(itemModel);
            }
        }

        private void OnButtonClick(CardPoolItemModel itemModel, int countOffset)
        {
            int nextCount = itemModel.count + countOffset;
            if (nextCount >= 0 && nextCount <= 9)
            {
                _cardPoolModel.ChangeTotalCount(itemModel.cardNumber, itemModel.cardColor, nextCount);
            }
        }

        public void UpdateBoardCardInfoList(IEnumerable<IGameBoardCardModel> boardCardModelList)
        {
            _cardPoolModel.UpdatePoolItemModelDict(boardCardModelList);
        }

        public void OnCardDeckPanelActive(bool value)
        {
            _cardPoolUI.gameObject.SetActive(value);
            if (value)
            {
                _cardPoolUI.UpdateUI(_cardPoolModel.PoolItemModelDictReadOnly, _cardPoolModel.TotalItemModelDictReadOnly);
            }
        }

        public void UpdateCardDeckItemModelList(IEnumerable<CardDeckItemModel> deckItemModelList)
        {
            _cardPoolModel.UpdatePoolItemModelDict(deckItemModelList);
        }

        public string SaveLevelData()
        {
            CardPoolSaveData data = new()
            {
                itemDataList = _cardPoolModel.TotalItemModelDictReadOnly.Values.ToList()
            };
            
            return JsonUtility.ToJson(data);
        }

        public void RestoreLevelData(string data)
        {
            _cardPoolModel.RestoreSaveData(data);
        }

        public void NewLevelData()
        {
            _cardPoolModel.ResetTotalItemModel();
        }
    }
}
