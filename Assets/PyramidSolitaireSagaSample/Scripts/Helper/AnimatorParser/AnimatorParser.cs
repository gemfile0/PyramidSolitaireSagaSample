using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper
{
    [Serializable]
    public class AnimatorParser : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private int _animationIndex;
        [SerializeField] private AnimationClip _animation;
        [SerializeField] private int _triggerIndex;
        [SerializeField] private string _trigger;

        public string Trigger
        {
            get
            {
                string result = "";

                if (_animator != null && _animation != null)
                {
                    result = _trigger;
                }

                return result;
            }
        }

        public float Duration
        {
            get
            {
                float result = 0f;

                if (_animator != null)
                {
                    if (_animation != null)
                    {
                        result = _animation.length;
                    }

                    // 1. 애니메이터의 마지막 트랜지션이 Exit 상태로 완전히 빠져나가는 것을 기다릴 목적으로
                    // 2. 지난 두 프레임의 정도의 시간을 더해줍니다.
                    result += (Time.deltaTime * 2);
                }

                // Debug.Log("=== Duration : " + result);
                return result;
            }
        }

        private WaitForSeconds _waitForDuration;
        private float _targetDuration;

        public void SetTrigger()
        {
            Debug.Log($"SetTrigger : {Trigger}");
            _animator.SetTrigger(Trigger);
        }

        public void SetTrigger(string trigger)
        {
            _animator.SetTrigger(trigger);
        }

        public YieldInstruction WaitForDuration(float offset = 0f)
        {
            float targetDuration = Duration + offset;
            Debug.Log($"SetTrigger : {targetDuration}");
            if (targetDuration < 0)
            {
                targetDuration = 0;
            }

            if (this._targetDuration != targetDuration)
            {
                _waitForDuration = new WaitForSeconds(targetDuration);
                this._targetDuration = targetDuration;
            }

            return _waitForDuration;
        }

        public bool ContainsParameter(string paramName)
        {
            for (int i = 0; i < _animator.parameters.Length; i++)
            {
                var param = _animator.parameters[i];

                if (param.name == paramName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

