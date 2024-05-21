using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PyramidSolitaireSagaSample.System.Popup
{
    public abstract class BasePopup : MonoBehaviour
    {
        [Header("BasePopup")]
        [SerializeField] private float openDuration = 0f;

        [HideInInspector]
        public UnityEvent onClose
        {
            get;
            private set;
        } = new UnityEvent();

        public RectTransform CachedTransform
        {
            get
            {
                if (_cachedTransform == null)
                {
                    _cachedTransform = GetComponent<RectTransform>();
                }
                return _cachedTransform;
            }
        }
        private RectTransform _cachedTransform;

        private Coroutine _closeCoroutine;

        private void OnEnable()
        {
            if (openDuration > 0)
            {
                _closeCoroutine = StartCoroutine(CloseCoroutine());
            }
        }

        private IEnumerator CloseCoroutine()
        {
            yield return new WaitForSeconds(openDuration);
            Close();
        }

        public virtual void Close()
        {
            StopCloseCoroutine();

            onClose?.Invoke();
        }

        protected void StopCloseCoroutine()
        {
            if (_closeCoroutine != null)
            {
                StopCoroutine(_closeCoroutine);
                _closeCoroutine = null;
            }
        }

        internal IEnumerator WaitForClose()
        {
            while (gameObject.activeInHierarchy)
            {
                yield return null;
            }
        }
    }
}
