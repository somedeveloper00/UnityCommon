using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace UnityCommon
{
    public sealed class HashHelpers
    {
        public static int GetPrime(int min)
        {
            for (int i = 0; i < primes.Length; i++)
            {
                int prime = primes[i];
                if (prime >= min) return prime;
            }

            //outside of our predefined table. 
            //compute the hard way. 
            for (int i = min | 1; i < int.MaxValue; i += 2)
            {
                if (IsPrime(i))
                    return i;
            }
            return min;
        }

        public static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                int limit = (int)Mathf.Sqrt(candidate);
                for (int divisor = 3; divisor <= limit; divisor += 2)
                {
                    if ((candidate % divisor) == 0)
                        return false;
                }
                return true;
            }
            return candidate == 2;
        }

        internal static readonly int[] primes =
        {
                3, 7, 17, 37, 89, 197, 431, 919, 1931, 4049, 8419, 17519, 36353,
                75431, 156437, 324449, 672827, 1395263, 2893249, 5999471,
            };
    }

    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public abstract class SerializableHashSetBase<T> : ISet<T>
    {
        private const int Left31BitsMask = 0x7FFFFFFF;

        private readonly IEqualityComparer<T> comparer;
        private List<Slot>[] _slots;
        private int _count;

        #region ctors

        public SerializableHashSetBase() : this(0, EqualityComparer<T>.Default) { }

        public SerializableHashSetBase(IEqualityComparer<T> comparer) : this(0, comparer) { }

        public SerializableHashSetBase(IEnumerable<T> other) : this(other, EqualityComparer<T>.Default) { }

        public SerializableHashSetBase(int capacity) : this(capacity, EqualityComparer<T>.Default) { }

        public SerializableHashSetBase(IEnumerable<T> other, IEqualityComparer<T> comparer)
        {
            this.comparer = comparer;
            _slots = new List<Slot>[other.Count() / 2];
            foreach (var item in other)
            {
                Add(item);
            }
        }

        public SerializableHashSetBase(int capacity, IEqualityComparer<T> comparer)
        {
            if (capacity <= 0)
            {
                capacity = 4;
            }
            this.comparer = comparer;
            _slots = new List<Slot>[capacity];
        }

        #endregion

        public int Count => _count;

        public bool IsReadOnly => false;

        #region implicit implementations

        void ICollection<T>.Add(T item) => Add(item);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region explicit implementations

        public bool Add(T item)
        {
            var hash = GetHashInternal(item);
            int slotIndex = hash % _slots.Length;

            _slots[slotIndex] ??= new();
            var slots = _slots[slotIndex];
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].hashCode == hash)
                {
                    return false;
                }
            }
            // item is new
            int capacity = slots.Capacity;
            if (capacity == slots.Count)
            {
                slots.Capacity = HashHelpers.GetPrime(capacity);
            }
            slots.Add(new(hash, item));

            _count++;
            return true;
        }

        public bool Remove(T item)
        {
            var hash = GetHashInternal(item);
            var slotIndex = hash % _slots.Length;

            for (int i = 0; i < _slots[slotIndex]?.Count; i++)
            {
                if (_slots[slotIndex][i].hashCode == hash)
                {
                    _slots[slotIndex].RemoveAt(i);
                    _count--;
                    return true;
                }
            }
            return false;
        }

        public void Clear()
        {
            for (int i = 0; i < _slots.Length; i++)
                _slots[i]?.Clear();
            _count = 0;
        }

        public bool Contains(T item)
        {
            var hash = GetHashInternal(item);
            var slotIndex = hash % _slots.Length;
            var slots = _slots[slotIndex];
            for (int i = 0; i < slots?.Count; i++)
            {
                if (slots[i].hashCode == hash)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            foreach (var slots in _slots)
            {
                for (int i = 0; i < slots?.Count; i++)
                {
                    if (arrayIndex == array.Length)
                    {
                        return;
                    }
                    array[arrayIndex++] = slots[i].value;
                }
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (_count == 0)
            {
                return;
            }
            if (this == other)
            {
                Clear();
                return;
            }
            foreach (var item in other)
            {
                Remove(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var slots in _slots)
            {
                for (int i = 0; i < slots?.Count; i++)
                {
                    yield return slots[i].value;
                }
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (_count == 0)
            {
                return;
            }
            if (other == this)
            {
                return;
            }

            RemoveWhere(i => !other.Contains(i));
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (_count == 0)
            {
                return true;
            }
            if (this == other)
            {
                return false;
            }

            foreach (var slots in _slots)
            {
                for (int i = 0; i < slots?.Count; i++)
                {
                    if (!other.Contains(slots[i].value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return IsSubsetOf(other) && other.Count() != _count;
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (this == other)
            {
                return true;
            }

            foreach (var item in other)
            {
                if (!Contains(item))
                {
                    return false;
                }
            }
            return true;

        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return IsSupersetOf(other) && other.Count() != _count;
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (_count == 0)
            {
                return false;
            }

            foreach (T element in other)
            {
                if (Contains(element))
                {
                    return true;
                }
            }
            return false;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            if (this == other)
            {
                return true;
            }

            int c = 0;
            foreach (var item in other)
            {
                if (!Contains(item))
                {
                    return false;
                }
                c++;
            }
            return c == _count;
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            if (this == other)
            {
                return;
            }

            foreach (var item in other)
            {
                Remove(item);
            }

            foreach (var item in this.AsEnumerable())
            {
                if (other.Contains(item))
                {
                    Remove(item);
                }
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var item in other)
            {
                Add(item);
            }
        }

        #endregion

        public void RemoveWhere(Func<T, bool> callback)
        {
            foreach (var slots in _slots)
            {
                for (int i = 0; i < slots?.Count; i++)
                {
                    if (callback(slots[i].value))
                    {
                        slots.RemoveAt(i--);
                        _count--;
                    }
                }
            }
        }

        private int GetHashInternal(T item)
        {
            if (item == null)
            {
                return 0;
            }
            return comparer.GetHashCode(item) & Left31BitsMask;
        }

        private readonly struct Slot
        {
            public readonly int hashCode;
            public readonly T value;

            public Slot(int hashCode, T value)
            {
                this.hashCode = hashCode;
                this.value = value;
            }
        }
    }

    [Serializable]
    public sealed class SerializableHashSet<T> : SerializableHashSetBase<T>, ISerializationCallbackReceiver
    {
        [SerializeField] private T[] values;

        public SerializableHashSet() { }
        public SerializableHashSet(IEqualityComparer<T> comparer) : base(comparer) { }
        public SerializableHashSet(int capacity) : base(capacity) { }
        public SerializableHashSet(int capacity, IEqualityComparer<T> comparer) : base(capacity, comparer) { }
        public SerializableHashSet(IEnumerable<T> other) : base(other) { }
        public SerializableHashSet(IEnumerable<T> other, IEqualityComparer<T> comparer) : base(other, comparer) { }

        void ISerializationCallbackReceiver.OnBeforeSerialize() => values = this.ToArray();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();
            foreach (var item in values)
            {
                if (!Add(item))
                {
                    Add(default); // Unity's Inspector duplicates items
                }
            }
        }
    }

    [Serializable]
    public sealed class ReferenceSerializableHashSet<T> : SerializableHashSetBase<T>, ISerializationCallbackReceiver
    {
        [SerializeReference] private T[] values;

        public ReferenceSerializableHashSet() { }
        public ReferenceSerializableHashSet(IEqualityComparer<T> comparer) : base(comparer) { }
        public ReferenceSerializableHashSet(int capacity) : base(capacity) { }
        public ReferenceSerializableHashSet(int capacity, IEqualityComparer<T> comparer) : base(capacity, comparer) { }
        public ReferenceSerializableHashSet(IEnumerable<T> other) : base(other) { }
        public ReferenceSerializableHashSet(IEnumerable<T> other, IEqualityComparer<T> comparer) : base(other, comparer) { }

        void ISerializationCallbackReceiver.OnBeforeSerialize() => values = this.ToArray();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();
            foreach (var item in values)
            {
                if (!Add(item))
                {
                    Add(default); // Unity's Inspector duplicates items
                }
            }
        }
    }

    [Serializable]
    public sealed class TypedReferenceSerializableHashSet<T> : SerializableHashSetBase<T>, ISerializationCallbackReceiver
    {
        [SerializeReference] private T[] values;

        public TypedReferenceSerializableHashSet() : base(ByTypeComparer<T>.Instance) { }
        public TypedReferenceSerializableHashSet(int capacity) : base(capacity, ByTypeComparer<T>.Instance) { }
        public TypedReferenceSerializableHashSet(IEnumerable<T> other) : base(other, ByTypeComparer<T>.Instance) { }

        void ISerializationCallbackReceiver.OnBeforeSerialize() => values = this.ToArray();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();
            foreach (var item in values)
            {
                if (!Add(item))
                {
                    Add(default); // Unity's Inspector duplicates items
                }
            }
        }
    }
}