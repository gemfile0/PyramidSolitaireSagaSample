using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper
{
    [CustomEditor(typeof(AnimatorParser))]
    public class AnimatorParserInspector : Editor
    {
        private SerializedProperty _animator;
        private SerializedProperty _animationIndex;
        private SerializedProperty _animation;
        private SerializedProperty _triggerIndex;
        private SerializedProperty _trigger;

        private void OnEnable()
        {
            _animator = serializedObject.FindProperty("_animator");
            _animationIndex = serializedObject.FindProperty("_animationIndex");
            _animation = serializedObject.FindProperty("_animation");
            _triggerIndex = serializedObject.FindProperty("_triggerIndex");
            _trigger = serializedObject.FindProperty("_trigger");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(_animator);

            var animatorField = _animator.objectReferenceValue as Animator;
            if (animatorField == null) { return; }

            AnimatorController animatorController = (AnimatorController)animatorField.runtimeAnimatorController;

            if (animatorController != null)
            {
                // 애니메이터에 포함된 애니메이션 클립들을 추출해 선택 가능한 인스펙터 UI를 제공합니다.
                AnimationClip[] animationClips = animatorController.animationClips;

                if (animationClips.Length > 0)
                {
                    _animationIndex.intValue = EditorGUILayout.Popup("Selected Animation",
                                                                    _animationIndex.intValue, 
                                                                    animationClips.Select(finding => finding.name).ToArray());

                    if (_animationIndex.intValue < animationClips.Length)
                    {
                        _animation.objectReferenceValue = animationClips[_animationIndex.intValue];
                    }
                }

                // 애니메이터에 포함된 파라미터들을 추출해 선택 가능한 인스펙터 UI를 제공합니다.
                AnimatorControllerParameter[] parameters = animatorController.parameters;
                if (parameters.Length > 0)
                {
                    _triggerIndex.intValue = EditorGUILayout.Popup(
                        "Selected Trigger",
                        _triggerIndex.intValue,
                        parameters.Select(finding => finding.name).ToArray()
                    );
                    _trigger.stringValue = (string)(parameters[_triggerIndex.intValue].name);
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
