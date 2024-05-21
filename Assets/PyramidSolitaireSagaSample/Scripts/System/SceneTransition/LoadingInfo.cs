using System;
using System.Collections;

namespace PyramidSolitaireSagaSample.System.SceneTransition
{
    public class LoadingInfo
    {
        public string Name
        {
            get;
            private set;
        }
        public float Duration
        {
            get;
            private set;
        }
        public float Progress
        {
            get;
            private set;
        }
        public bool IsDone
        {
            get;
            private set;
        }
        private Action onDone;
        public string Error
        {
            get;
            private set;
        }

        public Func</* onProgress */Action<float>, /* onComplete */Action, /* onError */Action<string>, IEnumerator> Coroutine
        {
            get;
            private set;
        }

        public LoadingInfo(string name, float duration, Func<Action<float>, Action, Action<string>, IEnumerator> coroutine)
        {
            Name = name;
            Duration = duration;
            Coroutine = coroutine;
        }

        public void Reset()
        {
            Progress = 0f;
            IsDone = false;
            Error = "";
        }

        public void UpdateProgress(float value)
        {
            Progress = value;
        }

        public void UpdateDone(bool value)
        {
            IsDone = value;
            Progress = Duration;
            onDone?.Invoke();
        }

        public void UpdateError(string error)
        {
            Error = error;
        }
    }
}
