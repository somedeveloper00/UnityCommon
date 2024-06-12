using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TankGame.Client.Common
{
    /// <summary>
    /// Knob for a slider. Standalone, this can work as a slider system.
    /// </summary>
    public sealed class SliderKnobHandler : MonoBehaviour, IDragHandler
    {
        public Action<float> Changed;
        public State zeroState;
        public State oneState;
        public RectTransform knobView;
        public SlideOrientation orientation;
        public Fillable fillable;
        private RectTransform _rectTransform;

        private void Awake() => _rectTransform = GetComponent<RectTransform>();

        [ContextMenu("Capture Zero")]
        private void CaptureZero() => zeroState.anchoredPosition = knobView.anchoredPosition;

        [ContextMenu("Capture One")]
        private void CaptureOne() => oneState.anchoredPosition = knobView.anchoredPosition;

        [ContextMenu("Set Zero")]
        private void SetZero() => Set(0);

        [ContextMenu("Set Half")]
        private void SetHalf() => Set(0.5f);

        [ContextMenu("Set One")]
        private void SetOne() => Set(1);

        public void Set(float t)
        {
            knobView.anchoredPosition = Vector2.Lerp(zeroState.anchoredPosition, oneState.anchoredPosition, t);
            if (fillable)
            {
                fillable.SetValue(t, 1);
            }
            Changed?.Invoke(t);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, eventData.position, eventData.pressEventCamera, out var point);
            var rect = _rectTransform.rect;
            var min = orientation is SlideOrientation.Vertical ? rect.yMin : rect.xMin;
            var max = orientation is SlideOrientation.Vertical ? rect.yMax : rect.xMax;
            var t = orientation is SlideOrientation.Vertical ? point.y : point.x;
            Set(Mathf.InverseLerp(min, max, t));
        }

        public enum SlideOrientation : byte
        {
            Vertical,
            Horizontal
        }

        [Serializable]
        public struct State
        {
            public Vector2 anchoredPosition;
        }
    }
}