using UnityEngine;

namespace TankGame.Client.Common
{
    /// <summary>
    /// Copies a source camera into a camera on this game object
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public sealed class CopyCamera : MonoBehaviour
    {
        [Tooltip("Source to copy from")]
        public Camera sourceCamera;
        private Camera _camera;

        private void Reset()
        {
            sourceCamera = transform.parent == true ? transform.parent.GetComponentInParent<Camera>() : Camera.main;
        }

        private void Awake() => _camera = GetComponent<Camera>();

        private void LateUpdate()
        {
            _camera.fieldOfView = sourceCamera.fieldOfView;
        }
    }
}