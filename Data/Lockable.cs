using System;
using System.Runtime.CompilerServices;

namespace UnityCommon
{
    /// <summary>
    /// Hides a value behind a lock so that consumers can lock the value. (it's serializable)
    /// </summary>
    [Serializable]
    public struct lockable<T> where T : struct
    {
        /// <summary>
        /// Actual value
        /// </summary>
        public T value;

        /// <summary>
        /// Underlying lock counter
        /// </summary>
        public uint locks;

        public lockable(T value)
        {
            this.value = value;
            locks = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddLock() => locks++;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveLock() => locks--;

        public readonly bool IsLocked
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => locks > 0;
        }

#nullable enable
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T? GetNullable() => IsLocked ? null : value;
#nullable restore

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(lockable<T> @lock) => @lock.IsLocked;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(lockable<T> @lock) => @lock.IsLocked ? default : @lock.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator lockable<T>(T value) => new(value);
    }
}