using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Positive-unlimitable <see cref="float"/>.
    /// </summary>
    [Serializable]
    public struct uufloat : IComparable<uufloat>
    {
        [SerializeField] private bool unlimited;
        [SerializeField] private float value;

        public uufloat(float value)
        {
            this.value = value;
            unlimited = false;
        }

        private uufloat(bool unlimited, float value)
        {
            this.value = value;
            this.unlimited = unlimited;
        }

        public readonly static uufloat Zero = new(false, 0);
        public readonly static uufloat Unlimited = new(true, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsUnlimited() => unlimited;

        public override readonly bool Equals(object obj) => obj is uufloat u2 && this == u2;
        public override readonly int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(unlimited);
            hash.Add(value);
            return hash.ToHashCode();
        }

        public override readonly string ToString() => unlimited ? "unlimited" : value.ToString();
        public readonly string ToString(string format) => unlimited ? "unlimited" : value.ToString(format);

        public readonly int CompareTo(uufloat other) => this == other ? 0 : unlimited ? 1 : other.unlimited ? -1 : value.CompareTo(other.value);

        public static bool operator ==(uufloat u1, uufloat u2) => u1.unlimited & u2.unlimited || u1.value == u2.value;
        public static bool operator !=(uufloat u1, uufloat u2) => !(u1 == u2);
        public static bool operator <(uufloat left, uufloat right) => left.CompareTo(right) < 0;
        public static bool operator <=(uufloat left, uufloat right) => left.CompareTo(right) <= 0;
        public static bool operator >(uufloat left, uufloat right) => left.CompareTo(right) > 0;
        public static bool operator >=(uufloat left, uufloat right) => left.CompareTo(right) >= 0;
        public static uufloat operator -(uufloat u1, uufloat u2) => u1.unlimited ? Unlimited : u2.unlimited ? Zero : new(false, u1.value - u2.value);
        public static uufloat operator +(uufloat u1, uufloat u2) => new(u1.unlimited | u2.unlimited, u1.value + u2.value);

        public static uufloat operator *(uufloat u1, int i) => i == 0 ? Zero : u1.unlimited ? Unlimited : new(false, u1.value * i);
        public static uufloat operator *(int i, uufloat u1) => i == 0 ? Zero : u1.unlimited ? Unlimited : new(false, u1.value * i);

        public static implicit operator uufloat(float value) => new(value);
        public static implicit operator float(uufloat value) => value.unlimited ? float.MaxValue : value.value;
        public static implicit operator int(uufloat value) => value.unlimited ? int.MaxValue : (int)value.value;
    }
}