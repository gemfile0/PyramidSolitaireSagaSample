using DG.Tweening;
using UnityEngine;

namespace PyramidSolitaireSagaSample
{
    public class GameBoostersEditorUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelObject;
        [SerializeField] private CanvasGroup _panelCanvasGroup;
        [SerializeField] private RectTransform _loadingImageTransform;

        private Tween _rotationTween;

        private void Start()
        {
            _panelObject.SetActive(false);
        }

        internal void ShowLoading()
        {
            _panelObject.SetActive(true);
            _panelCanvasGroup.alpha = 0;
            _panelCanvasGroup.DOFade(1f, .5f);
            _rotationTween = _loadingImageTransform.DORotate(new Vector3(0, 0, -360), 2f, RotateMode.FastBeyond360)
                                                   .SetEase(Ease.Linear)
                                                   .SetLoops(-1, LoopType.Restart);
        }

        internal void HideLoading()
        {
            if (_rotationTween != null)
            {
                _rotationTween.Kill();
                _rotationTween = null;
            }

            _panelCanvasGroup.DOFade(0f, .5f)
                             .OnComplete(() => _panelObject.SetActive(false));
        }
    }
}
