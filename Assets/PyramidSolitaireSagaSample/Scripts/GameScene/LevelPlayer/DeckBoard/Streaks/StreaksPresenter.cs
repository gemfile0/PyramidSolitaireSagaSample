using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.LevelPlayer.CardCollector;
using PyramidSolitaireSagaSample.LevelPlayer.GameTutorial;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.Streaks
{
    public class StreaksPresenter : MonoBehaviour,
                                    ILevelTutorialUiCloner
    {
        [Header("Model")]
        [SerializeField] private StreaksModel _streaksModel;

        [Header("View")]
        [SerializeField] private StreaksRenderer _streaksRenderer;

        public LevelTutorialCloneType UiCloneType => LevelTutorialCloneType.Streaks;

        public event Action<int> onStreaksChanged
        {
            add { _streaksModel.onStreaksChanged += value; }
            remove { _streaksModel.onStreaksChanged -= value; }
        }

        private void OnEnable()
        {
            _streaksModel.onStreaksChanged += _streaksRenderer.UpdateRenderer;
        }

        private void OnDisable()
        {
            _streaksModel.onStreaksChanged -= _streaksRenderer.UpdateRenderer;
        }

        internal void UpdateCardCollectorItemModelList(IEnumerable<CardCollectorItemModel> cardCollectorItemModel)
        {
            int streaksCount = 0;
            //Debug.Log($"UpdateCardCollectorItemModelList : {cardCollectorItemModel.Count()}");
            foreach (CardCollectorItemModel itemModel in cardCollectorItemModel)
            {
                if (itemModel.type == CardCollectorTriggerType.GameBoard)
                {
                    streaksCount += 1;
                }
                else if (itemModel.type == CardCollectorTriggerType.CardDeck
                         && itemModel.subCardType != SubCardType.Taped)
                {
                    streaksCount = 0;
                }

                //Debug.Log($"{itemModel.type}, {streaksCount}");
            }

            _streaksModel.UpdateStreaks(streaksCount);
        }

        public void CloneUI(Transform uiCloneContainerTransform, UiCloneData uiCloneData)
        {
            StreaksRenderer clonedStreaksRenderer = Instantiate(_streaksRenderer, uiCloneContainerTransform, false);
            clonedStreaksRenderer.CachedTransform.position = _streaksRenderer.CachedTransform.position;

        }
    }
}
