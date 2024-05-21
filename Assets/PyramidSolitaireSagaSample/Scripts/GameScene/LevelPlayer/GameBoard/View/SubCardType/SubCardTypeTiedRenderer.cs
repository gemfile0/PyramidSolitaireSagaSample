using DG.Tweening;
using PyramidSolitaireSagaSample.Helper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameBoard
{
    public class SubCardTypeTiedRenderer : BaseSubCardTypeRenderer
    {
        [SerializeField] private List<SpriteRenderer> _tiedRendererList;
        [SerializeField] private List<AnimatorParser> _tiedAnimatorParserList;

        private Color _latestColor;
        private int _tiedRendererIndex;
        private Dictionary<int, Coroutine> _coroutineDictionary;

        private void Awake()
        {
            _coroutineDictionary = new();
            foreach (SpriteRenderer tiedRenderer in _tiedRendererList)
            {
                tiedRenderer.gameObject.SetActive(false);
            }
        }

        internal override void UpdateColor(Color value)
        {
            _latestColor = value;
            UpdateTiedRendererColor();
        }

        private void UpdateTiedRendererColor()
        {
            foreach (SpriteRenderer tiedRenderer in _tiedRendererList)
            {
                tiedRenderer.color = _latestColor;
            }
        }

        internal override void Unpack(float fadeDelay, float fadeDuration, Ease fadeEase)
        {
            StartUnpackCoroutine(0);
        }

        internal override void UpdateOption2(int option2, bool animate)
        {
            //Debug.Log("UpdateOption2: " + option2);
            if (option2 != -1)
            {
                _tiedRendererIndex = option2;

                if (_tiedRendererIndex < _tiedAnimatorParserList.Count)
                {
                    for (int i = 0; i < _tiedAnimatorParserList.Count; i++)
                    {
                        AnimatorParser tiedAniamtorParser = _tiedAnimatorParserList[i];
                        bool originActive = tiedAniamtorParser.gameObject.activeSelf;
                        bool nextActive = i < _tiedRendererIndex;
                        if (animate == true
                            && originActive == true
                            && nextActive == false)
                        {
                            StartUnpackCoroutine(i);
                        }
                        else
                        {
                            tiedAniamtorParser.gameObject.SetActive(nextActive);
                        }
                    }
                }
            }
        }

        private void StartUnpackCoroutine(int animatorParserIndex)
        {
            if (_coroutineDictionary.ContainsKey(animatorParserIndex) == false)
            {
                Coroutine coroutine = StartCoroutine(UnpackCoroutine(animatorParserIndex));
                _coroutineDictionary.Add(animatorParserIndex, coroutine);
            }
        }

        private IEnumerator UnpackCoroutine(int animatorParserIndex)
        {
            AnimatorParser tiedAnimatorParser = _tiedAnimatorParserList[animatorParserIndex];
            tiedAnimatorParser.SetTrigger();
            yield return tiedAnimatorParser.WaitForDuration();
            tiedAnimatorParser.gameObject.SetActive(false);

            _coroutineDictionary.Remove(animatorParserIndex);
        }
    }
}
