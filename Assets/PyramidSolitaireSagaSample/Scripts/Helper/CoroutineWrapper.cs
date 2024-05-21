using System.Collections;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper
{
    public class CoroutineWrapper
    {
        public bool IsRunning => _coroutine != null;

        private MonoBehaviour _owner;
        private Coroutine _coroutine;

        public CoroutineWrapper(MonoBehaviour owner)
        {
            _owner = owner;
        }

        public void Start(IEnumerator coroutine)
        {
            if (_coroutine != null)
            {
                _owner.StopCoroutine(_coroutine);
            }

            _coroutine = _owner.StartCoroutine(RunCoroutine(coroutine));
        }

        private IEnumerator RunCoroutine(IEnumerator coroutine)
        {
            yield return _owner.StartCoroutine(coroutine);
            _coroutine = null;
        }

        public void Stop()
        {
            if (_coroutine != null)
            {
                _owner.StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
    }
}
