using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.Helper.UI
{
    [RequireComponent(typeof(Button))]
    public class PingButton : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
#if UNITY_EDITOR
            EditorGUIUtility.PingObject(gameObject);
#endif
        }
    }
}
