using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.GameData
{
    public enum GameBoardToolType
    {
        None = -1,
        CardStack = 0,
        CardBrush = 1,
        CardMover = 2,
        CardEraser = 3,
    }

    [Serializable]
    public class GameBoardToolData
    {
        public GameBoardToolType Type;
        public Color highlightColor;
    }

    [CreateAssetMenu(menuName = "PyramidSolitaireSagaSample/Game Board Data")]
    public class GameBoardData : ScriptableObject
    {
        [SerializeField] private Vector2 _tileSize;
        [SerializeField] private int _rowCount = 8, _colCount = 16;
        [SerializeField] private List<GameBoardToolData> _toolDataList;

        [Header("나비 설정")]
        [SerializeField] private Sprite _butterflySprite;
        [SerializeField] private float _butterflyMoveDelay = 0.5f;

        public Vector2 TileSize => _tileSize;
        public Vector2 HalfTileSize => _tileSize / 2;
        public int RowCount => _rowCount;
        public int ColCount => _colCount;
        public int LastRowIndex => _rowCount - 2;
        public int LastColIndex => _colCount - 2;
        public IEnumerable<GameBoardToolData> ToolDataList => _toolDataList;

        public Sprite ButterflySprite => _butterflySprite;
        public float ButterflyMoveDelay => _butterflyMoveDelay;

        public Vector3 GetSnappedPosition(Vector2Int snappedIndex)
        {
            //Debug.Log($"GetSnappedPosition : {snappedIndex}, {position}");
            Vector2 snappingSize = HalfTileSize;
            Vector3 snappedPosition = new Vector3(
                snappedIndex.x * snappingSize.x + snappingSize.x / 2,
                snappedIndex.y * snappingSize.y + snappingSize.y / 2,
                0
            );
            return snappedPosition;
        }

        public (Vector2Int, bool) GetSnappedIndex(Vector3 position)
        {
            // A-2. Scene 뷰의 그리드와 맞추가 위해 위치 조정한 영향
            Vector2 snappingSize = HalfTileSize;
            Vector2 halfSnappingSize = snappingSize / 2;
            position = new Vector3(position.x - halfSnappingSize.x, position.y - halfSnappingSize.y, position.z);
            Vector2Int snappedIndex = new Vector2Int(
                Mathf.RoundToInt(position.x / snappingSize.x),
                Mathf.RoundToInt(position.y / snappingSize.y)
            );

            bool isOutOfGrid = false;
            if (snappedIndex.x < 0)
            {
                isOutOfGrid = true;
                snappedIndex.x = 0;
            }
            else if (snappedIndex.x > LastColIndex)
            {
                isOutOfGrid = snappedIndex.x > LastColIndex + 1;
                snappedIndex.x = LastColIndex;
            }

            if (snappedIndex.y < 0)
            {
                if (isOutOfGrid == false)
                {
                    isOutOfGrid = true;
                }
                snappedIndex.y = 0;
            }
            else if (snappedIndex.y > LastRowIndex)
            {
                if (isOutOfGrid == false)
                {
                    isOutOfGrid = snappedIndex.y > LastRowIndex + 1;
                }
                snappedIndex.y = LastRowIndex;
            }

            return (snappedIndex, isOutOfGrid);
        }
    }
}
