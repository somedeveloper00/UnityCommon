using System;
using TankGame.Client.Common;
using UnityEngine;

namespace TankGame.Clint.Common
{
    /// <summary>
    /// Causes <see cref="CameraShake"/> to shake. Needs at least one active <see cref="CameraShake"/> component in the game to work.
    /// </summary>
    public sealed class CameraShaker : MonoBehaviour
    {
        [Tooltip("Conditions to shake the camera")]
        public Condition conditions;

        [Tooltip("The power of the shake")]
        public float magnitudeMultiplier = 1;

        private void Start() => DoIf(Condition.OnStart);
        private void OnDestroy() => DoIf(Condition.OnDestroy);

        private void DoIf(Condition cond)
        {
            if ((conditions & cond) == cond)
            {
                enabled = true;
                CameraShake.ShakeAllInstancesX(magnitudeMultiplier);
            }
        }

        [Flags]
        public enum Condition : byte
        {
            OnStart = 1 << 0,
            OnDestroy = 1 << 2
        }
    }
}