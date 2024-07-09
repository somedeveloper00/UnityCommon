using UnityEngine;

namespace UnityCommon
{
    public sealed class TransformClamp : MonoBehaviour
    {
        public Bounds bounds;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(bounds.center, bounds.size);
        }

        private void LateUpdate()
        {
            var pos = transform.localPosition;
            pos.x = Mathf.Clamp(pos.x, bounds.min.x, bounds.max.x);
            pos.y = Mathf.Clamp(pos.y, bounds.min.y, bounds.max.y);
            pos.z = Mathf.Clamp(pos.z, bounds.min.z, bounds.max.z);
            transform.localPosition = pos;
        }
    }
}