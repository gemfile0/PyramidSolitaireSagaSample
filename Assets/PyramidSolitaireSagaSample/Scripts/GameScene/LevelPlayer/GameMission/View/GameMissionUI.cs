using PyramidSolitaireSagaSample.GameData;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameMission
{
    public class GameMissionUI : MonoBehaviour
    {
        [SerializeField] private RectTransform _panelTransform;
        [SerializeField] private List<GameMissionItemUI> _itemUiList;

        public RectTransform CachedCanvasTransform
        {
            get
            {
                if (_cachedCanvasTransform == null)
                {
                    _cachedCanvasTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
                }
                return _cachedCanvasTransform;
            }
        }
        private RectTransform _cachedCanvasTransform;
        public RectTransform PanelTransform => _panelTransform;

        private StringBuilder _countBuilder = new StringBuilder();

        private void Awake()
        {
            foreach (GameMissionItemUI itemUI in _itemUiList)
            {
                itemUI.gameObject.SetActive(false);
            }

            _countBuilder = new StringBuilder();
        }

        internal void UpdateItemUI(int itemIndexOffset, GameMissionSkin gameMissionSkin, List<GameMissionProgressInfo> missionProgressInfoList, bool isCountTextVisible)
        {
            for (int i = 0; i < missionProgressInfoList.Count; i++)
            {
                int index = itemIndexOffset + i;

                GameMissionProgressInfo progressInfo = missionProgressInfoList[i];
                GameMissionItemUI itemUI = _itemUiList[index];

                _countBuilder.Length = 0;
                _countBuilder.Append(progressInfo.current);
                _countBuilder.Append("/");
                _countBuilder.Append(progressInfo.total);

                itemUI.UpdateUI(gameMissionSkin,
                                isCompleted: progressInfo.current >= progressInfo.total,
                                isCountTextVisible,
                                countStr: _countBuilder.ToString());
                itemUI.gameObject.SetActive(true);
            }
        }

        internal void CloneUI(Transform uiContainerTransform, int missionIndex)
        {
            GameMissionItemUI gameMissionItemUI = _itemUiList[missionIndex];
            GameMissionItemUI clonedGameMissionItemUI = Instantiate(gameMissionItemUI, uiContainerTransform, false);
            clonedGameMissionItemUI.CachedTransform.sizeDelta = gameMissionItemUI.CachedTransform.sizeDelta;
            clonedGameMissionItemUI.CachedTransform.position = gameMissionItemUI.CachedTransform.position;
        }
    }
}
