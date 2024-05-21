using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.Helper.UI
{
    public class Pagenation : MonoBehaviour
    {
        [SerializeField] private ToggleGroup _toggleGroup;
        [SerializeField] private Toggle _togglePrefab;
        [SerializeField] private RectTransform _togglePoolRoot;

        public event Action<int> onPageIndexChanged;

        private GameObjectPool<Toggle> _togglePool;
        private List<Toggle> _toggleList;

        private void Awake()
        {
            _togglePool = new GameObjectPool<Toggle>(_togglePoolRoot, _togglePrefab.gameObject, defaultCapacity: 5);
            _toggleList = new List<Toggle>();
        }

        internal void Init(int pageCount)
        {
            for (int i = 0; i < pageCount; i++)
            {
                Toggle toggle = _togglePool.Get();
                toggle.transform.SetParent(_togglePoolRoot);
                toggle.group = _toggleGroup;
                toggle.onValueChanged.AddListener(OnValueChanged);
                _toggleList.Add(toggle);
            }
        }

        private void OnValueChanged(bool value)
        {
            if (value)
            {
                int pageIndex = _toggleList.FindIndex(toggle => toggle.isOn);
                onPageIndexChanged?.Invoke(pageIndex);
            }
        }

        internal void ChangeIndex(int toggleIndex)
        {
            Toggle toggle = _toggleList[toggleIndex];
            toggle.SetIsOnWithoutNotify(true);
        }
    }
}
