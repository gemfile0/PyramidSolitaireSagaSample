using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.LevelPlayer.BonusCardSequence;
using PyramidSolitaireSagaSample.RuntimeLevelEditor.CardDeck;
using PyramidSolitaireSagaSample.System;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.BonusCardSequence
{
    public class BonusCardSequenceEditorPresenter : MonoBehaviour,
                                                    IGameInputActionMapTrigger,
                                                    ILevelSavable
    {
        [Header("Data")]
        [SerializeField] private CardData _cardData;

        [Header("Model")]
        [SerializeField] private BonusCardSequenceModel _bonusCardSequenceModel;

        [Header("View")]
        [SerializeField] private CardDeckEditorUI _bonusCardSequenceUI;

        public event Action<IEnumerable<BonusCardSequenceItemModel>> onSequenceItemModelUpdated
        {
            add { _bonusCardSequenceModel.onSequenceItemModelUpdated += value; }
            remove { _bonusCardSequenceModel.onSequenceItemModelUpdated -= value; }
        }

        public event Action<BonusCardSequenceItemModel, bool> onSequenceItemModelSelected
        {
            add { _bonusCardSequenceModel.onSequenceItemModelSelected += value; }
            remove { _bonusCardSequenceModel.onSequenceItemModelSelected -= value; }
        }

        public event Action<IEnumerable<BonusCardSequenceItemModel>> onSequenceCountChanged
        {
            add { _bonusCardSequenceModel.onSequenceCountChanged += value; }
            remove { _bonusCardSequenceModel.onSequenceCountChanged -= value; }
        }

        public event Action<bool> onPanelActive
        {
            add { _bonusCardSequenceUI.onPanelActive += value; }
            remove { _bonusCardSequenceUI.onPanelActive -= value; }
        }

        public string RestoreLevelID => RestoreLevelIdPath.BonusCardSequence;

        public event Action<string> onEnableActionMap;
        public event Action onRevertActionMap;

        private void Awake()
        {
            _bonusCardSequenceUI.Init();
            _bonusCardSequenceUI.gameObject.SetActive(true);
        }

        private void Start()
        {
            //_bonusCardSequenceModel.ChangeSequenceCountWithoutNotify(0);
            UpdateItemUI(_bonusCardSequenceModel.SequenceItemModelList);
        }

        private void OnEnable()
        {
            _bonusCardSequenceModel.onSequenceItemModelRestored += UpdateItemUI;
            _bonusCardSequenceModel.onSequenceCardInfoChanged += UpdateItemUI;
            _bonusCardSequenceModel.onSequenceItemModelSelected += SelectItemUI;
            _bonusCardSequenceModel.onSequenceCountChanged += UpdateItemUI;

            _bonusCardSequenceUI.onPanelActive += OnPanelActive;
            _bonusCardSequenceUI.onItemSelected += _bonusCardSequenceModel.SelectItemModel;
            _bonusCardSequenceUI.onStepperClick += OnStepperClick;
            _bonusCardSequenceUI.onValueChanged += OnValueChanged;
        }

        private void OnDisable()
        {
            _bonusCardSequenceModel.onSequenceItemModelRestored -= UpdateItemUI;
            _bonusCardSequenceModel.onSequenceCardInfoChanged -= UpdateItemUI;
            _bonusCardSequenceModel.onSequenceItemModelSelected -= SelectItemUI;
            _bonusCardSequenceModel.onSequenceCountChanged -= UpdateItemUI;

            _bonusCardSequenceUI.onPanelActive -= OnPanelActive;
            _bonusCardSequenceUI.onItemSelected -= _bonusCardSequenceModel.SelectItemModel;
            _bonusCardSequenceUI.onStepperClick -= OnStepperClick;
            _bonusCardSequenceUI.onValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(int nextCount)
        {
            if (nextCount >= 0 && nextCount <= 99)
            {
                _bonusCardSequenceModel.ChangeSequenceCount(nextCount);
            }
            else
            {
                _bonusCardSequenceUI.RevertCountUI();
            }
        }

        private void OnStepperClick(int offset)
        {
            int nextCount = _bonusCardSequenceModel.SequenceItemModelCount + offset;
            if (nextCount >= 0 && nextCount <= 99)
            {
                _bonusCardSequenceModel.ChangeSequenceCount(nextCount);
            }
        }

        private void SelectItemUI(BonusCardSequenceItemModel itemModel, bool isSelected)
        {
            if (isSelected)
            {
                _bonusCardSequenceUI.SelectItemUI(itemModel.Index);
            }
            else
            {
                _bonusCardSequenceUI.DeselectItemUI(itemModel.Index);
            }
        }

        private void UpdateItemUI(IEnumerable<BonusCardSequenceItemModel> itemModelList)
        {
            //Debug.Log($"UpdateItemUI : {itemModelList.Count()}");
            _bonusCardSequenceUI.CloseItemUI();
            foreach (BonusCardSequenceItemModel itemModel in itemModelList)
            {
                UpdateItemUI(itemModel);
            }

            _bonusCardSequenceUI.UpdateCountUI("Bonus Card Sequence", itemModelList.Count());
        }

        private void UpdateItemUI(BonusCardSequenceItemModel itemModel)
        {
            int itemIndex = itemModel.Index;
            CommonCardInfo cardInfo = itemModel.CardInfo;
            //Debug.Log($"UpdateItemUI : {itemIndex}, {cardInfo}");
            Sprite itemSprite = _cardData.GetCardSprite(cardInfo.CardNumber, cardInfo.CardColor, cardInfo.CardFace, cardInfo.CardType);

            _bonusCardSequenceUI.UpdateItemUI(itemIndex, itemSprite);
        }

        private void OnPanelActive(bool value)
        {
            _bonusCardSequenceUI.UpdatePanelUI(value);
            _bonusCardSequenceUI.transform.SetAsLastSibling();

            if (value)
            {
                onEnableActionMap?.Invoke("CardDeck");
            }
            else
            {
                _bonusCardSequenceModel.DeselectSequenceItemModel();

                onRevertActionMap?.Invoke();
            }
        }

        public string SaveLevelData()
        {
            BonusCardSequenceSaveData data = new()
            {
                itemDataList = _bonusCardSequenceModel.SequenceItemModelList
                    .Select((BonusCardSequenceItemModel itemModel) => new BonusCardSequenceSaveItemData
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
            if (string.IsNullOrEmpty(data) == false)
            {
                _bonusCardSequenceModel.RestoreSaveData(data);
            }
        }

        public void NewLevelData()
        {
            _bonusCardSequenceModel.ClearItemModelList();
        }
    }
}
