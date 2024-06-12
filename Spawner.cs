using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Spawns a prefab
    /// </summary>
    public sealed class Spawner : MonoBehaviour
    {
        public GameObject toSpawn;
        public Transform parent;
        public bool useSelfParent;

        /// <summary>
        /// Instantiate the prefab
        /// </summary>
        public GameObject Spawn() => Instantiate();

        /// <summary>
        /// Instantiate the prefab and multiply its <see cref="Transform.localScale"/> by <paramref name="scaleMultiplier"/>
        /// </summary>
        public void Spawn(float scaleMultiplier) => Instantiate().transform.localScale *= scaleMultiplier;

        private GameObject Instantiate()
        {
            transform.GetPositionAndRotation(out var pos, out var rot);
            return Instantiate(toSpawn, pos, rot, useSelfParent ? transform.parent : parent);
        }
    }
}