using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardDepth
{
    public class CardDepthEditorTool : MonoBehaviour
    {
        [Header("Presenter")]
        [SerializeField] private GameBoardEditorPresenter _gameBoardEditorPresenter;

        [Header("View")]
        [SerializeField] private CardDepthEditorToolUI _cardDepthEditorToolUI;

        private IGameBoardCardModel _selectedCardModel;

        private void OnEnable()
        {
            _gameBoardEditorPresenter.onCardModelSelected += OnCardModelSelected;
        }

        private void OnDisable()
        {
            _gameBoardEditorPresenter.onCardModelSelected -= OnCardModelSelected;
        }

        private void OnCardModelSelected(IGameBoardCardModel selectedCardModel, bool isSelected)
        {
            if (isSelected)
            {
                CommonCardInfo cardInfo = selectedCardModel.CardInfo;
                _selectedCardModel = selectedCardModel;
                _cardDepthEditorToolUI.UpdateStackIndex(_selectedCardModel.StackIndex, _selectedCardModel.StackCount);
                _cardDepthEditorToolUI.onStackChangerClick += _selectedCardModel.ChangeCardStackIndex;
                _cardDepthEditorToolUI.onStackItemClick += _selectedCardModel.ClickCardStackIndex;
            }
            else
            {
                _cardDepthEditorToolUI.ReleaseStackIndex();
                _cardDepthEditorToolUI.onStackChangerClick -= _selectedCardModel.ChangeCardStackIndex;
                _cardDepthEditorToolUI.onStackItemClick -= _selectedCardModel.ClickCardStackIndex;
                _selectedCardModel = null;
            }

            _cardDepthEditorToolUI.gameObject.SetActive(isSelected);
        }

    }
}
