using Ews.Essentials.Data;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TankGame.Client.Common
{
    /// <summary>
    /// Listens for pointer drag on this UI element
    /// </summary>
    [DefaultExecutionOrder(-1)]
    [RequireComponent(typeof(CanvasRenderer), typeof(RectTransform))]
    public sealed class PointerDragListener : Graphic, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public float multiClickTimeThreshold = 0.5f;

        public delegate void DraggedDelegate(in NativeList<PointerDragData> pointers);

        /// <summary>
        /// a pointer started dragging
        /// </summary>
        public event DraggedDelegate DragStarted;

        /// <summary>
        /// a pointer dragged
        /// </summary>
        public event DraggedDelegate Dragged;

        /// <summary>
        /// a pointer finished dragging
        /// </summary> 
        public event DraggedDelegate DragFinished;

        private NativeList<PointerDragData> _dragData;

        protected override void OnEnable()
        {
            base.OnEnable();
            _dragData = new(4, Allocator.Persistent);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _dragData.Dispose();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_dragData.Length == _dragData.Capacity)
            {
                return;
            }

            var id = eventData.pointerId;
            _dragData.Add(new()
            {
                id = id,
                start = eventData.position,
            });
            DragStarted?.Invoke(_dragData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var id = eventData.pointerId;
            for (int i = 0; i < _dragData.Length; i++)
            {
                ref var element = ref _dragData.ElementAt(i);
                if (element.id == id)
                {
                    element.current = eventData.position;
                    element.delta = eventData.delta;
                    element.duration += Time.deltaTime;
                    Dragged?.Invoke(_dragData);
                    return;
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var id = eventData.pointerId;
            for (int i = 0; i < _dragData.Length; i++)
            {
                ref var element = ref _dragData.ElementAt(i);
                if (element.id == id)
                {
                    _dragData.RemoveAt(i);
                    DragFinished?.Invoke(_dragData);
                    return;
                }
            }
        }

        public struct PointerDragData
        {
            /// <summary>
            /// unique ID of the pointer
            /// </summary>
            public int id;

            /// <summary>
            /// Duration of the drag
            /// </summary>
            public float duration;

            /// <summary>
            /// Starting position of the drag (in screen)
            /// </summary>
            public Vector2 start;

            /// <summary>
            /// Current position of the drag (in screen)
            /// </summary>
            public Vector2 current;

            /// <summary>
            /// Change in position since last frame
            /// </summary>
            public Vector2 delta;
        }
    }
}