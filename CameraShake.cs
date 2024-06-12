using System;
using System.Collections.Generic;
using Ews.Essentials.Data;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace TankGame.Client.Common
{
    /// <summary>
    /// A minimal and performant camera shaker. It manipulates the camera's transform in <see cref="LateUpdate"/> phase 
    /// and expects other scripts to set its real (unshaken) positin before this phase (so that if this phase did nothing, camera 
    /// would be at it's unshaken position). 
    /// Uses Unity's Collection to reach peak performance with minimal code structure.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraShake : MonoBehaviour
    {
        /// <summary>
        /// current active instances
        /// </summary>
        public readonly static List<CameraShake> Instances = new(1);

        [SerializeField]
        [Tooltip("Shake Info to use when calling " + nameof(DefaultShake) + "() method")]
        private ShakeInfo[] defaultShake = new ShakeInfo[]
        {
            new(1, 10, 2, 0),
            new(1, 10, 2, 0.1f),
            new(0.2f, 1, 1, 0.2f),
        };

        /// <summary>
        /// active shakes.
        /// </summary>
        private flist16<ShakeRuntimeInfo> _shakes;

        private void Awake()
        {
            Instances.Add(this);
        }

        private void OnDestroy()
        {
            Instances.Remove(this);
        }

        /// <summary>
        /// Shake all <see cref="Instances"/>
        /// </summary>
        public static void ShakeAllInstances()
        {
            foreach (var ins in Instances)
            {
                ins.DefaultShake();
            }
        }

        /// <summary>
        /// <inheritdoc cref="ShakeAllInstances"/> (with x times the magnitudes)
        /// </summary>
        public static void ShakeAllInstancesX(float x)
        {
            foreach (var ins in Instances)
            {
                ins.DefaultShakeX(x);
            }
        }

        /// <summary>
        /// Shake using the <see cref="defaultShake"/>
        /// </summary>
        public void DefaultShake() => AddShake(defaultShake);

        /// <summary>
        /// <inheritdoc cref="DefaultShake"/> (with x times the magnitues)
        /// </summary>
        public void DefaultShakeX(float x)
        {
            Assert.IsTrue(x > 0, $"{nameof(x)} should be more than zero");

            for (int i = 0; i < defaultShake.Length; i++)
            {
                ref var shake = ref defaultShake[i];
                shake.magnitudePos *= x;
                shake.magnitudeRot *= x;
            }
            AddShake(defaultShake);
            for (int i = 0; i < defaultShake.Length; i++)
            {
                ref var shake = ref defaultShake[i];
                shake.magnitudePos /= x;
                shake.magnitudeRot /= x;
            }
        }

        /// <summary>
        /// Add a new set of shake infos
        /// </summary>
        public void AddShake(ShakeInfo[] shakeInfos)
        {
            for (int i = 0; i < shakeInfos.Length; i++)
            {
                _shakes.Add(new(shakeInfos[i])
                {
                    shakePos = new()
                    {
                        x = Random.Range(-shakeInfos[i].magnitudePos, shakeInfos[i].magnitudePos),
                        y = Random.Range(-shakeInfos[i].magnitudePos, shakeInfos[i].magnitudePos),
                        z = Random.Range(-shakeInfos[i].magnitudePos, shakeInfos[i].magnitudePos),
                    },
                    shakeRot = new()
                    {
                        x = Random.Range(-shakeInfos[i].magnitudePos, shakeInfos[i].magnitudePos),
                        y = Random.Range(-shakeInfos[i].magnitudePos, shakeInfos[i].magnitudePos),
                        z = Random.Range(-shakeInfos[i].magnitudePos, shakeInfos[i].magnitudePos),
                    }
                });
            }
            enabled = true; // receive LateUpdate
        }

        private void LateUpdate()
        {
            transform.GetLocalPositionAndRotation(out var pos, out var rotQ);
            var rot = rotQ.eulerAngles;
            for (int i = 0; i < _shakes.Count; i++)
            {
                ref var s = ref _shakes[i];
                s.t += Time.deltaTime;

                if (s.t < s.shakeInfo.time) // not yet started
                    continue;

                // tick
                s.shakePos = Vector3.MoveTowards(s.shakePos, Vector3.zero, s.shakeInfo.calmSpeed * Time.deltaTime);
                s.shakeRot = Vector3.MoveTowards(s.shakeRot, Vector3.zero, s.shakeInfo.calmSpeed * Time.deltaTime);

                if (s.shakeRot == Vector3.zero && s.shakePos == Vector3.zero) // finished
                {
                    _shakes.RemoveAt(i--);
                    continue;
                }

                // apply
                pos += s.shakePos;
                rot += s.shakeRot;
            }
            transform.SetLocalPositionAndRotation(pos, Quaternion.Euler(rot));

            if (_shakes.Count == 0)
            {
                // don't receive LateUpdate anymore
                enabled = false;
            }
        }

        private struct ShakeRuntimeInfo
        {
            public readonly ShakeInfo shakeInfo;

            /// <summary>
            /// active shake pos
            /// </summary>
            public Vector3 shakePos;

            /// <summary>
            /// active shake rot
            /// </summary>
            public Vector3 shakeRot;

            /// <summary>
            /// current timer
            /// </summary>
            public float t;

            public ShakeRuntimeInfo(ShakeInfo shakeInfo) : this() => this.shakeInfo = shakeInfo;
        }

        [Serializable]
        public struct ShakeInfo
        {
            [Tooltip("power of shake for position")]
            public float magnitudePos;

            [Tooltip("power of shake for rotation")]
            public float magnitudeRot;

            [Tooltip("speed of canceling this shake")]
            public float calmSpeed;

            [Tooltip("delay of starting this shake")]
            public float time;

            public ShakeInfo(float magnitudePos, float magnitudeRot, float calmSpeed, float time)
            {
                this.magnitudePos = magnitudePos;
                this.magnitudeRot = magnitudeRot;
                this.calmSpeed = calmSpeed;
                this.time = time;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(CameraShake))]
        private sealed class CameraShakeEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                CameraShake camShake = (CameraShake)target;
                GUILayout.Label($"shakes count: {camShake._shakes.Count}");
                if (Application.isPlaying)
                {
                    if (GUILayout.Button("Default Shake"))
                        camShake.DefaultShake();
                }
                else
                {
                    using (new UnityEditor.EditorGUI.DisabledScope(true))
                        GUILayout.Button(new GUIContent("Default Shake", "Enter Play Mode to test camera shake"));
                }
            }
        }
#endif
    }
}