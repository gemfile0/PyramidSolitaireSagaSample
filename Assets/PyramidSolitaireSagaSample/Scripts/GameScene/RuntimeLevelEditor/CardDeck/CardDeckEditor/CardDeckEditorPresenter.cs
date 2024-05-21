using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.Helper;
using PyramidSolitaireSagaSample.LevelPlayer.CardDeck;
using PyramidSolitaireSagaSample.System;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardDeck
{
    public class CardDeckEditorPresenter : MonoBehaviour,
                                           IGameInputActionMapTrigger,
                                           ILevelSavable
    {
        public enum State
        {
            None,
            Restore,
        }

        [Header("Data")]
        [SerializeField] private CardData _cardData;

        [Header("Model")]
        [SerializeField] private CardDeckModel _cardDeckModel;

        [Header("View")]
        [SerializeField] private CardDeckEditorUI _cardDeckUI;

        public event Action<IEnumerable<CardDeckItemModel>> onItemModelRestored
        {
            add { _cardDeckModel.onItemModelRestored += value; }
            remove { _cardDeckModel.onItemModelRestored -= value; }
        }

        public event Action<IEnumerable<CardDeckItemModel>> onItemModelChanged
        {
            add { _cardDeckModel.onItemModelChanged += value; }
            remove { _cardDeckModel.onItemModelChanged -= value; }
        }

        public event Action<CardDeckItemModel, bool> onItemModelSelected
        {
            add { _cardDeckModel.onItemModelSelected += value; }
            remove { _cardDeckModel.onItemModelSelected -= value; }
        }

        public event Action<bool> onPanelActive
        {
            add { _cardDeckUI.onPanelActive += value; }
            remove { _cardDeckUI.onPanelActive -= value; }
        }

        public event Action<string> onEnableActionMap;
        public event Action onRevertActionMap;
        public string RestoreLevelID => RestoreLevelIdPath.CardDeck;

        private EnumState<State> _state;

        private void Awake()
        {
            _state = new();

            _cardDeckUI.Init();
            _cardDeckUI.gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            _cardDeckModel.onItemModelRestored += UpdateItemUI;
            _cardDeckModel.onCardInfoChanged += UpdateItemUI;
            _cardDeckModel.onItemModelSelected += SelectItemUI;

            _cardDeckUI.onPanelActive += OnPanelActive;
            _cardDeckUI.onItemSelected += _cardDeckModel.SelectItemModel;
        }

        private void OnDisable()
        {
            _cardDeckModel.onItemModelRestored -= UpdateItemUI;
            _cardDeckModel.onCardInfoChanged -= UpdateItemUI;
            _cardDeckModel.onItemModelSelected -= SelectItemUI;

            _cardDeckUI.onPanelActive -= OnPanelActive;
            _cardDeckUI.onItemSelected -= _cardDeckModel.SelectItemModel;
        }

        private void OnPanelActive(bool value)
        {
            _cardDeckUI.UpdatePanelUI(value);
            _cardDeckUI.transform.SetAsLastSibling();

            if (value)
            {
                onEnableActionMap?.Invoke("CardDeck");
            }
            else
            {
                _cardDeckModel.DeselectItemModel();

                onRevertActionMap?.Invoke();
            }
        }

        public void UpdateCardPoolDeckCount(int deckCount)
        {
            if (_state.CurrState == State.None)
            {
                _cardDeckModel.ChangeDeckCount(deckCount);
                UpdateItemUI(_cardDeckModel.ItemModelList);
            }
        }

        private void SelectItemUI(CardDeckItemModel itemModel, bool isSelected)
        {
            if (isSelected)
            {
                _cardDeckUI.SelectItemUI(itemModel.Index);
            }
            else
            {
                _cardDeckUI.DeselectItemUI(itemModel.Index);
            }
        }

        private void UpdateItemUI(IEnumerable<CardDeckItemModel> itemModelList)
        {
            _cardDeckUI.CloseItemUI();
            foreach (CardDeckItemModel itemModel in itemModelList)
            {
                UpdateItemUI(itemModel);
            }

            _cardDeckUI.UpdateCountUI("Card Deck", itemModelList.Count());
        }

        private void UpdateItemUI(CardDeckItemModel itemModel)
        {
            int itemIndex = itemModel.Index;

            CommonCardInfo cardInfo = itemModel.CardInfo;
            //Debug.Log($"UpdateItemUI : {itemIndex}, {cardInfo}");
            Sprite itemSprite = _cardData.GetCardSprite(cardInfo.CardNumber, cardInfo.CardColor, cardInfo.CardFace, cardInfo.CardType);

            _cardDeckUI.UpdateItemUI(itemIndex, itemSprite);
        }

        public string SaveLevelData()
        {
            CardDeckSaveData data = new()
            {
                itemDataList = _cardDeckModel.ItemModelList
                    .Select((CardDeckItemModel itemModel) => new CardDeckSaveItemData
                    {
                        ID = itemModel.Index,
                        CardInfo = itemModel.CardInfo
                    })
                    .ToList()
            };

            //Debug.Log($"GetData : {data.itemDataList.Count}");
            return JsonUtility.ToJson(data);
        }

        public void RestoreLevelData(string data)
        {
            _state.Set(State.Restore);
            if (string.IsNullOrEmpty(data) == false)
            {
                _cardDeckModel.RestoreSaveData(data);
            }
            _state.Set(State.None);
        }

        public void NewLevelData()
        {
            _cardDeckModel.ClearItemModelList();
        }
    }
}
