using DG.Tweening;
using UnityEngine;

namespace PyramidSolitaireSagaSample.GameData
{
    [CreateAssetMenu(menuName = "PyramidSolitaireSagaSample/Ui Movement Data")]
    public class UiMovementData : ScriptableObject
    {
        [SerializeField] private float _missionUiMoveDuration = 1f;
        [SerializeField] private Ease _missionUiMoveEase = Ease.InOutQuad;

        public float MissionUiMoveDuration => _missionUiMoveDuration;
        public Ease MissionUiMoveEase => _missionUiMoveEase;
    }
}
