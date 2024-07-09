using System;

namespace UnityCommon
{
    /// <summary>
    /// An item in a <see cref="IManifest{T}"/> without any references. It holds the ID of the item in the 
    /// manifest.
    /// </summary>
    [Serializable]
    public struct ManifestItem<T>
    {
        /// <summary>
        /// The underlying/serialized ID of this item in the manifest.
        /// </summary>
        public int id;

        /// <summary>
        /// Get the actual item from the manifest.
        /// </summary>
        public readonly T GetFromManifest(IManifest<T> manifest) => manifest.GetItem(id);

        /// <summary>
        /// Sets the underlying item from the manifest.
        /// </summary>
        public void SetFromManifest(IManifest<T> manifest, T item) => id = manifest.GetIdOfItem(item);
    }

    /// <summary>
    /// A manifest holds a set of items and makes it convinient for reference-free systems to 
    /// hold items from a manifest completely unmanaged.
    /// </summary>
    public interface IManifest<TItem>
    {
        int GetIdOfItem(TItem item);
        TItem GetItem(int id);
    }
}