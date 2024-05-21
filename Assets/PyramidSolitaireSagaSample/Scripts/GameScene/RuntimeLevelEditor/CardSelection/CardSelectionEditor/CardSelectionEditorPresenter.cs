using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardSelection
{
    public class CardSelectionEditorPresenter : MonoBehaviour
    {
        [Header("Model")]
        [SerializeField] private CardSelectionEditorModel _cardSelectionModel;

        [Header("View")]
        [SerializeField] private CardSelectionEditorUI _cardSelectionUI;

        private void OnEnable()
        {
            _cardSelectionModel.onItemUpdated += UpdateUI;
        }

        private void OnDisable()
        {
            _cardSelectionModel.onItemUpdated -= UpdateUI;
        }

        private void UpdateUI(ICardSelectionItemModel itemModel, bool isSelected)
        {
            if (isSelected)
            {
                CommonCardInfo cardInfo = itemModel.CardInfo;
                _cardSelectionUI.UpdateUI(
                    cardInfo.CardNumber,
                    cardInfo.CardColor,
                    cardInfo.CardFace,
                    cardInfo.CardType,
                    cardInfo.SubCardType,
                    cardInfo.SubCardTypeOption,
                    cardInfo.IsBonusLabel
                );
                _cardSelectionUI.onCardSelectionInfoChanged += itemModel.UpdateCardInfo;
            }
            else
            {
                _cardSelectionUI.ResetUI();
                _cardSelectionUI.onCardSelectionInfoChanged -= itemModel.UpdateCardInfo;
            }
        }

        public void UpdateCardDeckItemModel(ICardSelectionItemModel itemModel, bool isSelected)
        {
            _cardSelectionModel.UpdateItemModel(itemModel, isSelected);
            _cardSelectionUI.SetActive(panelActive: isSelected,
                                       numberActive: true,
                                       colorActive: true,
                                       false, false, false, SubCardType.SubType_None);
        }

        public void UpdateGameBoardCardModel(ICardSelectionItemModel itemModel, bool isSelected)
        {
            _cardSelectionModel.UpdateItemModel(itemModel, isSelected);
            _cardSelectionUI.SetActive(panelActive: isSelected,
                                       numberActive: true,
                                       colorActive: true,
                                       faceActive: true,
                                       typeActive: true,
                                       subCardTypeOptionActive: _cardSelectionUI.IsSubCardTypeOptionActive(itemModel.CardInfo.SubCardType),
                                       subCardType: itemModel.CardInfo.SubCardType);
        }

        internal void UpdateBonusCardSequenceItemModel(ICardSelectionItemModel itemModel, bool isSelected)
        {
            _cardSelectionModel.UpdateItemModel(itemModel, isSelected);
            _cardSelectionUI.SetActive(panelActive: isSelected,
                                       numberActive: true,
                                       colorActive: true,
                                       false, false, false, SubCardType.SubType_None);
        }
    }
}
