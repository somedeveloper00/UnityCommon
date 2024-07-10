using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// A burst-compatible simple collision system to replace Unity's default collision system. 
    /// Useful for when you need collision checks but don't want to include a whole physics module in 
    /// the build.
    /// </summary>
    [BurstCompile]
    public sealed class FastCollider : MonoBehaviour
    {
        [SerializeField]
        private SphereEntry[] _sphereEntries = new SphereEntry[]
        {
            new(){ offset = float3.zero, radius = 10}
        };
        [SerializeField]
        private BoxEntry[] _boxEntries = new BoxEntry[] { };

        private NativeList<SphereEntry> runtime_sphereEntries;
        private NativeList<BoxEntry> runtime_boxEntries;

        private void Awake()
        {
            runtime_sphereEntries = new(_sphereEntries.Length, Allocator.Persistent);
            runtime_boxEntries = new(_boxEntries.Length, Allocator.Persistent);
            for (int i = 0; i < _sphereEntries.Length; i++)
                runtime_sphereEntries.Add(_sphereEntries[i]);
            for (int i = 0; i < _boxEntries.Length; i++)
                runtime_boxEntries.Add(_boxEntries[i]);
        }

        private void OnDestroy()
        {
            if (runtime_boxEntries.IsCreated)
                runtime_boxEntries.Dispose();
            if (runtime_sphereEntries.IsCreated)
                runtime_sphereEntries.Dispose();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new(1, 0, 0, 0.5f);
            if (Application.isPlaying)
                Draw(runtime_sphereEntries.ToArray(), runtime_boxEntries.ToArray());
            else
                Draw(_sphereEntries, _boxEntries);

            void Draw(SphereEntry[] spheres, BoxEntry[] boxes)
            {
                foreach (var entry in spheres)
                    Gizmos.DrawSphere(transform.position + (Vector3)entry.offset, entry.radius);
                foreach (var entry in boxes)
                    Gizmos.DrawCube(transform.position + (Vector3)entry.offset, entry.size);
            }
        }

        public bool CollidesWith(FastCollider other)
        {
            return collisionCheck(
                transform.position, runtime_sphereEntries, runtime_boxEntries,
                other.transform.position, other.runtime_sphereEntries, other.runtime_boxEntries);
        }

        public bool SphereRaycast(float3 center, float radius)
        {
            var s1 = new NativeList<SphereEntry>(1, Allocator.Temp) { new() { radius = radius } };
            var b1 = new NativeList<BoxEntry>(0, Allocator.Temp);
            bool result = collisionCheck(center, s1, b1, transform.position, runtime_sphereEntries, runtime_boxEntries);
            s1.Dispose();
            b1.Dispose();
            return result;
        }

        public bool BoxRaycast(float3 center, float3 size)
        {
            var s1 = new NativeList<SphereEntry>(0, Allocator.Temp);
            var b1 = new NativeList<BoxEntry>(1, Allocator.Temp) { new() { offset = 0, size = size } };
            bool result = collisionCheck(center, s1, b1, transform.position, runtime_sphereEntries, runtime_boxEntries);
            s1.Dispose();
            b1.Dispose();
            return result;
        }

        [BurstCompile]
        private bool collisionCheck(
                in float3 c1, in NativeList<SphereEntry> s1, in NativeList<BoxEntry> b1,
                in float3 c2, in NativeList<SphereEntry> s2, in NativeList<BoxEntry> b2)
        {
            foreach (var _s1 in s1)
            {
                foreach (var _s2 in s2)
                {
                    if (collides(c1, _s1, c2, _s2))
                    {
                        return true;
                    }
                }
                foreach (var _b2 in b2)
                {
                    if (collides(c1, _s1, c2, _b2))
                    {
                        return true;
                    }
                }
            }
            foreach (var _b1 in b1)
            {
                foreach (var _s2 in s2)
                {
                    if (collides(c2, _s2, c1, _b1))
                    {
                        return true;
                    }
                }
                foreach (var _b2 in b2)
                {
                    if (collides(c2, _b2, c1, _b1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool collides(in float3 pos1, in SphereEntry c1, in float3 pos2, in BoxEntry c2)
        {
            float3 closestPoint = default;
            float3 boxMin = pos2 + c2.offset - c2.size * 0.5f;
            float3 boxMax = pos2 + c2.offset + c2.size * 0.5f;
            float3 sphereCent = pos1 + c1.offset;

            for (int i = 0; i < 3; i++)
                closestPoint[i] = math.clamp(sphereCent[i], boxMin[i], boxMax[i]);

            return collides(pos1, c1, closestPoint, new SphereEntry() { offset = 0, radius = 0 });
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool collides(in float3 pos1, in BoxEntry c1, in float3 pos2, in BoxEntry c2)
        {
            var cent1 = pos1 + c1.offset;
            var cent2 = pos2 + c2.offset;
            var hsize1 = c1.size * 0.5f;
            var hsize2 = c2.size * 0.5f;

            return
                Mathf.Abs(cent1.x - cent2.x) < (hsize1.x + hsize2.x) &&
                Mathf.Abs(cent1.y - cent2.y) < (hsize1.y + hsize2.y) &&
                Mathf.Abs(cent1.z - cent2.z) < (hsize1.z + hsize2.z);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool collides(in float3 pos1, in SphereEntry c1, in float3 pos2, in SphereEntry c2)
        {
            return math.lengthsq(pos2 + c2.offset - pos1 - c1.offset) <= (c2.radius + c1.radius) * (c2.radius + c1.radius);
        }

        [DebuggerDisplay("Offset = {offset} Radius = {radius}")]
        [Serializable]
        private struct SphereEntry
        {
            public float3 offset;
            public float radius;
        }

        [DebuggerDisplay("Offset = {offset} Size = {size}")]
        [Serializable]
        private struct BoxEntry
        {
            public float3 offset;
            public float3 size;
        }
    }
}