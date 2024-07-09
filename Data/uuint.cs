using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Positive-unlimitable <see cref="int"/>.
    /// </summary>
    [Serializable]
    public struct uuint : IComparable<uuint>
    {
        [SerializeField] private bool unlimited;
        [SerializeField] private int value; // not using uint because Unity doesn't support it in inspector

        public uuint(int value)
        {
            this.value = value;
            unlimited = false;
        }

        private uuint(bool unlimited, int value)
        {
            this.value = value;
            this.unlimited = unlimited;
        }

        public readonly static uuint Zero = new(false, 0);
        public readonly static uuint Unlimited = new(true, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsUnlimited() => unlimited;

        public override readonly bool Equals(object obj) => obj is uuint u2 && this == u2;
        public override readonly int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(unlimited);
            hash.Add(value);
            return hash.ToHashCode();
        }

        public override readonly string ToString() => unlimited ? "unlimited" : value.ToString();
        public readonly string ToString(string format) => unlimited ? "unlimited" : value.ToString(format);

        public readonly int CompareTo(uuint other) => this == other ? 0 : unlimited ? 1 : other.unlimited ? -1 : value.CompareTo(other.value);

        public static bool operator ==(uuint u1, uuint u2) => u1.unlimited & u2.unlimited || u1.value == u2.value;
        public static bool operator !=(uuint u1, uuint u2) => !(u1 == u2);
        public static bool operator <(uuint left, uuint right) => left.CompareTo(right) < 0;
        public static bool operator <=(uuint left, uuint right) => left.CompareTo(right) <= 0;
        public static bool operator >(uuint left, uuint right) => left.CompareTo(right) > 0;
        public static bool operator >=(uuint left, uuint right) => left.CompareTo(right) >= 0;
        public static uuint operator -(uuint u1, uuint u2) => u1.unlimited ? Unlimited : u2.unlimited ? Zero : new(false, u1.value - u2.value);
        public static uuint operator +(uuint u1, uuint u2) => new(u1.unlimited | u2.unlimited, u1.value + u2.value);

        public static uuint operator *(uuint u1, int i) => i == 0 ? Zero : u1.unlimited ? Unlimited : new(false, u1.value * (int)i);
        public static uuint operator *(int i, uuint u1) => i == 0 ? Zero : u1.unlimited ? Unlimited : new(false, u1.value * (int)i);

        public static implicit operator uuint(int value) => new(value);
        public static implicit operator int(uuint value) => value.unlimited ? int.MaxValue : value.value;
        public static implicit operator float(uuint value) => value.unlimited ? float.MaxValue : value.value;

    }
}