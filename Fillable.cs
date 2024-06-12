using System;
using AnimFlex.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TankGame.Client.Common
{
    /// <summary>
    /// A common slider for showing skills in a linear line that gets filled
    /// </summary>
    public sealed class Fillable : MonoBehaviour
    {
        [SerializeField] private RectTransform filler;
        [SerializeField] private Image fillerImage;
        [SerializeField] private State emptyState, filledState;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private string valueFormat = "{0}";
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private Ease ease = Ease.InOutSine;
        private float _previousT;
        private Tweener<float> _tweener;
        private float _lastValue;

        private void Reset()
        {
            filler = GetComponent<RectTransform>();
            fillerImage = GetComponent<Image>();
        }

        public void SetValue(float value, float max)
        {
            if (value == _lastValue)
            {
                return;
            }
            _lastValue = value;

            if (valueText)
            {
                valueText.SetText(valueFormat, value);
            }

            value = Mathf.Clamp(value, 0, max);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SetInternal(value / max);
                return;
            }
#endif

            if (_tweener?.IsActive() == true)
                _tweener.Kill(false, false);

            _tweener = Tweener.Generate(() => _previousT, SetInternal, (float)(value / max),
                duration, 0, ease, null, () => this);

        }

        [ContextMenu("Set Fill")] private void SetFill() => SetInternal(1);
        [ContextMenu("Set 20%")] private void Set20() => SetInternal(0.2f);
        [ContextMenu("Set Half")] private void SetHalf() => SetInternal(0.5f);
        [ContextMenu("Set 80%")] private void Set80() => SetInternal(0.8f);
        [ContextMenu("Set Empty")] private void SetEmpty() => SetInternal(0);

        private void SetInternal(float value)
        {
            _previousT = value;
            if (filler)
            {
                filler.anchorMin = Vector3.Lerp(emptyState.anchorMin, filledState.anchorMin, value);
                filler.anchorMax = Vector3.Lerp(emptyState.anchorMax, filledState.anchorMax, value);
                filler.anchoredPosition = Vector2.Lerp(emptyState.anchoredPosition, filledState.anchoredPosition, value);
                filler.sizeDelta = Vector2.Lerp(emptyState.sizeDelta, filledState.sizeDelta, value);
            }

            if (fillerImage)
            {
                fillerImage.color = Color.Lerp(emptyState.color, filledState.color, value);
            }
        }

        [Serializable]
        public struct State
        {
            public Vector2 anchoredPosition;
            public Vector2 sizeDelta;
            public Vector2 anchorMax;
            public Vector2 anchorMin;
            public Color color;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomPropertyDrawer(typeof(State))]
        private class RectTransformStateDrawer : UnityEditor.PropertyDrawer
        {
            readonly float vspace = UnityEditor.EditorGUIUtility.standardVerticalSpacing;
            readonly float sh = UnityEditor.EditorGUIUtility.singleLineHeight;
            const float wspace = 10;

            public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
            {
                using (new UnityEditor.EditorGUI.PropertyScope(position, label, property))
                {
                    position.height = sh;
                    position.y += vspace;

                    // drop down
                    property.isExpanded = UnityEditor.EditorGUI.BeginFoldoutHeaderGroup(new(position.x, position.y, position.width - 82, position.height),
                        property.isExpanded, label);
                    if (GUI.Button(new(position.x + position.width - 80, position.y, 80, position.height), "Capture"))
                    {
                        Capture();
                    }
                    UnityEditor.EditorGUI.EndFoldoutHeaderGroup();
                    if (!property.isExpanded)
                        return;


                    UnityEditor.EditorGUI.indentLevel++;
                    position.y += position.height + vspace;
                    UnityEditor.EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(State.anchoredPosition)));
                    position.y += position.height + vspace;
                    UnityEditor.EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(State.sizeDelta)));
                    position.y += position.height + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
                    UnityEditor.EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(State.color)));
                    position.y += position.height + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
                    UnityEditor.EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(State.anchorMin)));
                    position.y += position.height + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
                    UnityEditor.EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(State.anchorMax)));
                    position.y += position.height + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
                    UnityEditor.EditorGUI.indentLevel--;

                }

                void Capture()
                {
                    var gameObject = (property.serializedObject.targetObject as Component)?.gameObject;
                    if (!gameObject)
                    {
                        Debug.LogWarning("Could not find game object");
                        return;
                    }
                    if (!gameObject.TryGetComponent<Fillable>(out var fillable))
                    {
                        Debug.LogWarningFormat("Could not find a {0} component", nameof(Fillable));
                        return;
                    }
                    if (fillable.filler == null)
                    {
                        Debug.LogWarningFormat("{0} field on the game object {1} is empty.", nameof(filler), fillable.name);
                        return;
                    }

                    property.FindPropertyRelative(nameof(State.anchoredPosition)).vector2Value = fillable.filler.anchoredPosition;
                    property.FindPropertyRelative(nameof(State.sizeDelta)).vector2Value = fillable.filler.sizeDelta;
                    property.FindPropertyRelative(nameof(State.anchorMin)).vector2Value = fillable.filler.anchorMin;
                    property.FindPropertyRelative(nameof(State.anchorMax)).vector2Value = fillable.filler.anchorMax;
                    if (fillable.fillerImage == null)
                    {
                        Debug.LogWarningFormat("{0} field on the game object {1} is empry, so color could not be captured.", nameof(fillerImage), fillable.name);
                        return;
                    }
                    property.FindPropertyRelative(nameof(State.color)).colorValue = fillable.fillerImage.color;
                }
            }

            public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
                => property.isExpanded ? sh * 6 + vspace * 7 : sh + vspace;
        }
#endif
    }
}