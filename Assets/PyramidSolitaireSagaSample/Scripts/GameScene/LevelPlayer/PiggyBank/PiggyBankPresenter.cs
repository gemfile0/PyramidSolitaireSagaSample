using PyramidSolitaireSagaSample.LevelPlayer.LootAnimation;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.PiggyBank
{
    public class PiggyBankPresenter : MonoBehaviour, ILootAnimationEndPoint
    {
        [SerializeField] private PiggyBankModel _piggyBankModel;
        [SerializeField] private PiggyBankUI _piggyBankUI;

        public void BeginLootAnimation()
        {
            _piggyBankUI.Show();
        }

        public void EndLootAnimation(long bonusCount)
        {
            _piggyBankUI.Hide();
        }

        public Vector3 GetLootAnimtionEndPoint()
        {
            return _piggyBankUI.EndPoint;
        }
    }
}
