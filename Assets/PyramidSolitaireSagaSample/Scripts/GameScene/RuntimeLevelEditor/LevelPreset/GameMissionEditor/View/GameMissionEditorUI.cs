using PyramidSolitaireSagaSample.Helper;
using PyramidSolitaireSagaSample.Helper.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameMission
{
    public class GameMissionEditorUI : MonoBehaviour
    {
        [SerializeField] private Transform _itemGroupRoot;
        [SerializeField] private GameMissionEditorItemGroup _itemGroupPrefab;
        [SerializeField] private Button addButton;

        public event Action onAddMissionClick;
        public event Action<int> onRemoveMissionClick;
        public event Action<int, GameMissionType> onMissionTypeChanged;
        public event Action<int, int> onMissionCountChanged;
        public event Action<int, int> onMissionStepperClick;

        private GameObjectPool<GameMissionEditorItemGroup> _itemGroupPool;
        private List<GameMissionEditorItemGroup> _itemGroupList;
        private int _missionCount;

        private LayoutRebuilderInvoker _layoutRebuilderInvoker;

        public void Init()
        {
            _itemGroupPool = new GameObjectPool<GameMissionEditorItemGroup>(parent: _itemGroupRoot, prefab: _itemGroupPrefab.gameObject, defaultCapacity: 5);
            _itemGroupList = new List<GameMissionEditorItemGroup>();
            _missionCount = 5;
            _layoutRebuilderInvoker = GetComponent<LayoutRebuilderInvoker>();
        }

        public void OnAddClick()
        {
            onAddMissionClick?.Invoke();
        }

        internal void AddMissionItemGroup(GameMissionItemModel itemModel)
        {
            GameMissionEditorItemGroup itemGroup = _itemGroupPool.Get();
            itemGroup.Init(
                itemGroupIndex: _itemGroupList.Count, 
                missionTypeValue: (int)itemModel.type, 
                missionCount: itemModel.count, 
                onRemoveMissionClick, 
                onMissionTypeChanged,
                onMissionCountChanged,
                onMissionStepperClick
            );
            _itemGroupList.Add(itemGroup);

            itemGroup.CachedRectTransform.SetParent(_itemGroupRoot, false);
            itemGroup.CachedRectTransform.SetSiblingIndex(_itemGroupList.Count);

            UpdateAddButtonInteractable();

            _layoutRebuilderInvoker.UpdateInvokerList();
        }

        internal void RemoveMissionItemGroup(int itemGroupIndex)
        {
            GameMissionEditorItemGroup itemGroup = _itemGroupList[itemGroupIndex];
            _itemGroupPool.Release(itemGroup);
            _itemGroupList.RemoveAt(itemGroupIndex);

            for (int i = 0; i < _itemGroupList.Count; i++)
            {
                GameMissionEditorItemGroup _itemGroup = _itemGroupList[i];
                _itemGroup.UpdateItemGroupIndex(i);
            }

            UpdateAddButtonInteractable();
        }

        internal void ClearMissionItemGroup()
        {
            foreach (GameMissionEditorItemGroup itemGroup in _itemGroupList)
            {
                _itemGroupPool.Release(itemGroup);
            }
            _itemGroupList.Clear();
        }

        private void UpdateAddButtonInteractable()
        {
            addButton.interactable = _itemGroupList.Count < _missionCount;
        }

        internal void RevertItemGroupUI(int itemGroupIndex)
        {
            GameMissionEditorItemGroup itemGroup = _itemGroupList[itemGroupIndex];
            itemGroup.RevertUI();
        }

        internal void UpdateUI(int itemGroupIndex, GameMissionType type, int count)
        {
            GameMissionEditorItemGroup itemGroup = _itemGroupList[itemGroupIndex];
            itemGroup.UpdateUI((int)type, count);
        }
    }
}
