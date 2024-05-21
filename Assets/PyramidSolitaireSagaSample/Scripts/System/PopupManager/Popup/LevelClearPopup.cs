using System;

namespace PyramidSolitaireSagaSample.System.Popup
{
    public class LevelClearPopup : AnimationPopup
    {
        private Action _onReplayCurrentLevelClick;
        private Action _onPlayNextLevelClick;

        public void Init(Action onReplayCurrentLevelClick, Action onPlayNextLevelClick)
        {
            _onReplayCurrentLevelClick = onReplayCurrentLevelClick;
            _onPlayNextLevelClick = onPlayNextLevelClick;
        }

        public void ReplayCurrentLevelClick()
        {
            _onReplayCurrentLevelClick?.Invoke();
            Close();
        }

        public void PlayNextLevelClick()
        {
            _onPlayNextLevelClick?.Invoke();
            Close();
        }

        public override void Close()
        {
            base.Close();

            _onReplayCurrentLevelClick = null;
            _onPlayNextLevelClick = null;
        }
    }
}
