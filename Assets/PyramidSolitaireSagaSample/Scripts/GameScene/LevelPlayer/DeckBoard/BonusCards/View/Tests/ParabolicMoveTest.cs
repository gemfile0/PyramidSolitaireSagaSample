using DG.Tweening;
using UnityEngine;

namespace PyramidSolitaireSagaSample
{
    public class ParabolicMoveTest : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float height = 2.0f;
        [SerializeField] private float duration = 5.0f;

        void Start()
        {
            MoveInParabola();
        }

        void MoveInParabola()
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = target.position;

            Vector3 midPoint = (startPos + endPos) / 2 + Vector3.up * height;
            Vector3[] path = new Vector3[] { startPos, midPoint, endPos };
            transform.DOPath(path, duration, PathType.CatmullRom).SetEase(Ease.InOutQuad);
        }
    }
}
