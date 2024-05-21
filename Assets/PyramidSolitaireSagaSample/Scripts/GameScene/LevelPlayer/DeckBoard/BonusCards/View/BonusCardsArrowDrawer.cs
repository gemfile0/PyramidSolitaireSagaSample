using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.BonusCards
{
    [RequireComponent(typeof(LineRenderer))]
    public class BonusCardsArrowDrawer : MonoBehaviour
    {
        [SerializeField] private float _arrowHeadLength = 1f;
        [SerializeField] private float _arrowHeadAngle = 30.0f;
        [SerializeField] private float _arrowWidth = 0.25f;

        private LineRenderer _lineRenderer;
        private List<Vector3> _positions;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _positions = new List<Vector3>();
        }

        public void DrawArrow(Vector2 startPoint, Vector2 endPoint)
        {
            Vector2 direction = (endPoint - startPoint).normalized;

            Vector2 right = Quaternion.Euler(0, 0, _arrowHeadAngle) * -direction * _arrowHeadLength;
            Vector2 left = Quaternion.Euler(0, 0, -_arrowHeadAngle) * -direction * _arrowHeadLength;

            _positions.Clear();
            _positions.Add(new Vector3(startPoint.x, startPoint.y, 0));
            _positions.Add(new Vector3(endPoint.x, endPoint.y, 0));
            _positions.Add(new Vector3(endPoint.x + right.x, endPoint.y + right.y, 0));
            _positions.Add(new Vector3(endPoint.x + left.x, endPoint.y + left.y, 0));
            _positions.Add(new Vector3(endPoint.x, endPoint.y, 0));

            _lineRenderer.positionCount = _positions.Count;
            _lineRenderer.startWidth = _arrowWidth;
            _lineRenderer.endWidth = _arrowWidth;
            _lineRenderer.SetPositions(_positions.ToArray());
            _lineRenderer.loop = false;
        }

        internal void UpdateSortingOrder(int value)
        {
            _lineRenderer.sortingOrder = value;
        }
    }
}
