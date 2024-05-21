using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.Helper.UI
{
    public interface ILayoutRebuilderInvoker
    {
        event Action requestLayoutRebuild;
    }

    public class LayoutRebuilderInvoker : MonoBehaviour
    {
        private ILayoutRebuilderInvoker[] _layoutRebuilderInvokerList;

        private void OnEnable()
        {
            UnlistenInvokerList();
        }

        public void UpdateInvokerList()
        {
            ListenInvokerList();
            UnlistenInvokerList();
        }

        private void UnlistenInvokerList()
        {
            _layoutRebuilderInvokerList = GetComponentsInChildren<ILayoutRebuilderInvoker>();
            foreach (ILayoutRebuilderInvoker invoker in _layoutRebuilderInvokerList)
            {
                invoker.requestLayoutRebuild += OnRequestLayoutRebuild;
            }
        }

        private void OnDisable()
        {
            ListenInvokerList();
        }

        private void ListenInvokerList()
        {
            foreach (ILayoutRebuilderInvoker invoker in _layoutRebuilderInvokerList)
            {
                invoker.requestLayoutRebuild -= OnRequestLayoutRebuild;
            }
            _layoutRebuilderInvokerList = null;
        }

        private void OnRequestLayoutRebuild()
        {
            StartCoroutine(LayoutRebuildCoroutine());
        }

        private IEnumerator LayoutRebuildCoroutine()
        {
            yield return null;
            //Debug.Log($"LayoutRebuildCoroutine : {gameObject.name}");
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }
    }
}
