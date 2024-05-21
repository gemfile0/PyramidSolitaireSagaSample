using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace PyramidSolitaireSagaSample.GameData.Tests
{
    public class BonusCardsDataTest
    {
        private BonusCardsData _bonusCardsData;

        [SetUp]
        public void Setup()
        {
            var assetPath = "Assets/PyramidSolitaireSagaSample/GameDatas/BonusCardsData.asset";
            _bonusCardsData = AssetDatabase.LoadAssetAtPath<BonusCardsData>(assetPath);
            Assert.IsNotNull(_bonusCardsData, $"에셋 로드에 실패했습니다 : {assetPath}");
        }

        [Test]
        public void TestGenerateBonusCard_AccordingToProbability()
        {
            _TestGenerateBonusCard_AccordingToProbability(_bonusCardsData.BonusCardProbabilityDatas, _bonusCardsData.GenerateBonusCardType);
        }

        [Test]
        public void TestGenerateBonusLabelCard_AccrodingToProbability()
        {
            _TestGenerateBonusCard_AccordingToProbability(_bonusCardsData.BonusLabelCardProbabilityDatas, _bonusCardsData.GenerateBonusLabelCardType);
        }

        private void _TestGenerateBonusCard_AccordingToProbability(BonusCardProbabilityData[] probabilityDatas, Func<(int, BonusCardType[])> _GenerateBonusCardType)
        {
            // 1. Arrange
            int testIterations = 10000;
            Dictionary<int, int> cardCounts = new Dictionary<int, int>();
            for (int dataIndex = 0; dataIndex < probabilityDatas.Length; dataIndex++)
            {
                cardCounts[dataIndex] = 0;
            }

            // 2. Act
            for (int i = 0; i < testIterations; i++)
            {
                (int dataIndex, BonusCardType[] cardTypes) = _GenerateBonusCardType.Invoke();
                cardCounts[dataIndex] += 1;
            }

            // 3. Assert
            for (int dataIndex = 0; dataIndex < probabilityDatas.Length; dataIndex++)
            {
                BonusCardProbabilityData data = probabilityDatas[dataIndex];
                float expectedCount = testIterations * data.probability / 100f;
                float actualCount = cardCounts[dataIndex];
                float tolerance = 0.05f * expectedCount; // 5% tolerance

                Assert.IsTrue(
                    actualCount >= expectedCount - tolerance && actualCount <= expectedCount + tolerance,
                    $"Data index {dataIndex} does not match expected probability distribution. " +
                    $"Expected range: {expectedCount - tolerance} to {expectedCount + tolerance}, " +
                    $"but was {actualCount}."
                );
                UnityEngine.Debug.Log($"Data index {dataIndex} : Expected range: {expectedCount - tolerance} to {expectedCount + tolerance}, Actual {actualCount}");
            }
        }

    }
}
