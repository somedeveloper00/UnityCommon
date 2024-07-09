using AnimFlex.Tweening;
using Ews.Essentials.Data;
using UnityEngine;

namespace UnityCommon
{
    [DefaultExecutionOrder(-1)] 
    public sealed class PanObject : MonoBehaviour
    {
        public bool Dragging { get; private set; }

        private Tweener<Vector3> _falloffTweener;
        [SerializeField] private PointerDragListener pointerDragListener;
        [SerializeField] private float speed;
        [SerializeField] private float stopDuration;
        private Vector3 _lastVelocity;

        private void OnEnable()
        {
            pointerDragListener.DragStarted += DragStarted;
            pointerDragListener.Dragged += Dragged;
            pointerDragListener.DragFinished += DragFinished;
        }
        private void OnDisable()
        {
            pointerDragListener.DragStarted -= DragStarted;
            pointerDragListener.Dragged -= Dragged;
            pointerDragListener.DragFinished -= DragFinished;
            if (_falloffTweener?.IsActive() == true)
                _falloffTweener.Kill();
        }

        private void DragStarted(in flist8<PointerDragListener.PointerDragData> pointers)
        {
            Dragging = true;
            if (_falloffTweener?.IsActive() == true)
                _falloffTweener.Kill();
        }

        private void Dragged(in flist8<PointerDragListener.PointerDragData> pointers)
        {
            var delta = pointers[0].delta;
            _lastVelocity = (Vector3)(speed * delta);
        }

        private void DragFinished(in flist8<PointerDragListener.PointerDragData> pointers)
        {
            Dragging = false;
            _falloffTweener = this.AnimRef(ref _lastVelocity, Vector3.zero, Ease.OutSine, stopDuration);
        }

        private void LateUpdate()
        {
            if (_lastVelocity != Vector3.zero)
            {
                transform.localPosition += _lastVelocity * Time.deltaTime;
            }
        }
    }
}