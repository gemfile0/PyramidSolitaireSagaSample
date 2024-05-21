using PyramidSolitaireSagaSample.Helper;
using System.Collections;
using UnityEngine;

namespace PyramidSolitaireSagaSample.System.Popup
{
    public class AnimationPopup : BasePopup
    {
        [Header("AnimationPopup")]
        [SerializeField] private AnimatorParser _animatorParser;

        public override void Close()
        {
            StopCloseCoroutine();
            StartCoroutine(CloseAnimationCoroutine());
        }

        private IEnumerator CloseAnimationCoroutine()
        {
            _animatorParser.SetTrigger();
            yield return _animatorParser.WaitForDuration();

            base.Close();
        }
    }
}
