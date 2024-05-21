using PyramidSolitaireSagaSample.System.LevelDataManager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.BonusCardSequence
{
    public interface IBonusCardSequencePicker
    {
        void AddBonusCard(int bonusCount);
        int ItemModelCount { get; }
        void DrawPeekItemModel(SubCardType consumedSubCardType);
        void RevealPeekItemModel();
    }

    public class BonusCardSequencePresenter : MonoBehaviour,
                                              ILevelRestorable,
                                              IBonusCardSequencePicker
    {
        [SerializeField] private BonusCardSequenceModel _bonusCardSequenceModel;

        public event Action<IEnumerable<BonusCardItemModel>, int> onBonusCardCountAdded
        {
            add { _bonusCardSequenceModel.onBonusCardCountAdded += value; }
            remove { _bonusCardSequenceModel.onBonusCardCountAdded -= value; }
        }

        public event Action<CardNumber, CardColor, SubCardType> onItemModelDrawn
        {
            add { _bonusCardSequenceModel.onItemModelDrawn += value; }
            remove { _bonusCardSequenceModel.onItemModelDrawn -= value; }
        }

        public event Action<CardNumber, CardColor> onItemModelRevealed
        {
            add { _bonusCardSequenceModel.onItemModelRevealed += value; }
            remove { _bonusCardSequenceModel.onItemModelRevealed -= value; }
        }

        public event Action<int> onBonusCardCountUpdated
        {
            add { _bonusCardSequenceModel.onBonusCardCountUpdated += value; }
            remove { _bonusCardSequenceModel.onBonusCardCountUpdated -= value; }
        }

        public int ItemModelCount => _bonusCardSequenceModel.ItemModelCount;

        public string RestoreLevelID => RestoreLevelIdPath.BonusCardSequence;

        public void AddBonusCard(int bonusCount)
        {
            _bonusCardSequenceModel.AddBonusCard(bonusCount);
        }

        public void DrawPeekItemModel(SubCardType consumedSubCardType)
        {
            _bonusCardSequenceModel.DrawPeekItemModel(consumedSubCardType);
        }

        public void RestoreLevelData(string data)
        {
            _bonusCardSequenceModel.RestoreSaveData(data);
        }

        public void RevealPeekItemModel()
        {
            _bonusCardSequenceModel.RevealPeekItemModel();
        }
    }
}
