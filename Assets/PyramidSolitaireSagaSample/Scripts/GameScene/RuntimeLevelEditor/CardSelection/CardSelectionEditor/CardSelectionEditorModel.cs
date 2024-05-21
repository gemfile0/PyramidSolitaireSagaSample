using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardSelection
{
    public interface ICardSelectionItemModel
    {
        public int ID { get; }
        public CommonCardInfo CardInfo { get; }

        public void UpdateCardInfo(CardSelectionInfo cardSelectionInfo);
    }

    public class CardSelectionEditorModel : MonoBehaviour
    {
        public event Action<ICardSelectionItemModel, bool> onItemUpdated;

        private ICardSelectionItemModel _itemModel;
        private bool _isSelected;

        internal void UpdateItemModel(ICardSelectionItemModel itemModel, bool isSelected)
        {
            //Debug.Log($"UpdateItemModel : {itemModel.ID}, {itemModel.CardInfo}");

            _itemModel = itemModel;
            _isSelected = isSelected;

            onItemUpdated?.Invoke(_itemModel, _isSelected);
        }
    }
}
