using UnityEngine;

namespace PyramidSolitaireSagaSample
{
    public class RotationTest : MonoBehaviour
    {
        [SerializeField] private Transform cardTransform;
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Vector3 axis = Vector3.forward;

        private void Update()
        {
            Vector3 direction = (targetTransform.position - cardTransform.position).normalized;
            float radians = Mathf.Atan2(direction.y, direction.x);
            float angle = radians * Mathf.Rad2Deg;
            cardTransform.rotation = Quaternion.AngleAxis(angle - 90, axis);
        }
    }
}
