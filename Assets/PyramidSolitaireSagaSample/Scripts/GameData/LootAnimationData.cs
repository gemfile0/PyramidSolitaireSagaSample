using DG.Tweening;
using UnityEngine;

namespace PyramidSolitaireSagaSample.GameData
{
    [CreateAssetMenu(menuName = "Solitaire Makeover/Loot Animation Data")]
    public class LootAnimationData : ScriptableObject
    {
        [Header("전리품 애니메이션 이동 설정")]
        [SerializeField] private float _spawnDelay = .05f;
        [SerializeField] private float _jumpHeight = .3f;
        [SerializeField] private float _moveDuration = 3.0f;
        [SerializeField] private Ease _moveEase = Ease.InOutQuad;
        [SerializeField] private PathType _pathType = PathType.CatmullRom;
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private Ease _fadeEase = Ease.OutQuint;
        [SerializeField] private float _textDuration = 1f;
        [SerializeField] private Ease _textEase = Ease.OutQuint;
        [SerializeField] private float _textStartOffsetY = .5f;
        [SerializeField] private float _textEndOffsetY = 1.5f;

        public float SpawnDelay => _spawnDelay;
        public float JumpHeight => _jumpHeight;
        public float MoveDuration => _moveDuration;
        public Ease MoveEase => _moveEase;
        public PathType PathType => _pathType;
        public float FadeDuration => _fadeDuration;
        public Ease FadeEase => _fadeEase;
        public float TextDuration => _textDuration;
        public Ease TextEase => _textEase;
        public float TextStartOffsetY => _textStartOffsetY;
        public float TextEndOffsetY => _textEndOffsetY;
    }
}
