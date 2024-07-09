using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityCommon
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PointerPanRect : Graphic, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Vector2 DragDelta { get; private set; }
        public Vector2 StartPosition { get; private set; }
        public int PointerId { get; private set; } = -1;
        public bool Panning { get; private set; }
        private bool _lock;

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            color = Color.clear;
        }
#endif

        private void LateUpdate()
        {
            if (!_lock)
            {
                DragDelta = Vector2.zero;
            }
            _lock = false;
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (PointerId == -1)
            {
                PointerId = eventData.pointerId;
                StartPosition = eventData.position;
                DragDelta = Vector2.zero;
                Panning = true;
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == PointerId)
            {
                DragDelta = eventData.position - StartPosition;
                _lock = true;
                Panning = true;
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == PointerId)
            {
                DragDelta = Vector2.zero;
                PointerId = -1;
                Panning = false;
            }
        }
    }
}