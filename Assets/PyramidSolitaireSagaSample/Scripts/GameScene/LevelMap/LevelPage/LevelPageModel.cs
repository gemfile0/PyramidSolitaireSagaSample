using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelMap.LevelPage
{
    public class LevelPageModel : MonoBehaviour
    {
        public event Action<IEnumerable<string>> onPathListUpdated;

        private List<string> _pathList;

        internal void UpdatePathList(List<string> pathList)
        {
            _pathList = pathList;
            onPathListUpdated?.Invoke(_pathList);
        }
    }
}
