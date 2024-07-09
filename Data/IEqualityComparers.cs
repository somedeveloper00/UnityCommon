using System.Collections.Generic;

namespace UnityCommon
{
    public readonly struct ByTypeComparer<T> : IEqualityComparer<T>
    {
        public static readonly ByTypeComparer<T> Instance = new();

        public readonly bool Equals(T x, T y) => x.GetType() == y.GetType();

        public readonly int GetHashCode(T obj) => obj is null ? 2 : obj.GetType().GetHashCode();
    }
}