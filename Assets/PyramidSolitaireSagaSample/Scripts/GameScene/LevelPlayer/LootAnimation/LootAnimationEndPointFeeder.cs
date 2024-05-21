using PyramidSolitaireSagaSample.LevelPlayer.CardDeck;
using PyramidSolitaireSagaSample.LevelPlayer.JokerDeck;
using PyramidSolitaireSagaSample.LevelPlayer.PiggyBank;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.LootAnimation
{
    public class LootAnimationEndPointFeeder : MonoBehaviour
    {
        [SerializeField] private LootAnimationController _lootAnimationController;
        [SerializeField] private CardDeckPresenter _cardDeckPresenter;
        [SerializeField] private PiggyBankPresenter _piggyBankPresenter;
        [SerializeField] private JokerDeckPresenter _jokerDeckPresenter;

        private void Start()
        {
            _lootAnimationController.AddLootAnimationEndPoint(LootAnimationType.QuestionCard, _cardDeckPresenter);
            _lootAnimationController.AddLootAnimationEndPoint(LootAnimationType.Coin, _piggyBankPresenter);
            _lootAnimationController.AddLootAnimationEndPoint(LootAnimationType.JokerCard, _jokerDeckPresenter);
        }
    }
}
