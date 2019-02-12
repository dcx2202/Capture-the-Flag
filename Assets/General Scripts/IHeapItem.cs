using System;

namespace General_Scripts
{
    /// <summary>
    ///  1 - higher priority
    ///  0 - same priority
    /// -1 - lower priority
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHeapItem<T> : IComparable<T>
    {
        /// <summary>
        /// The index of this item on its heap.
        /// </summary>
        int HeapIndex { get; set; }

    }
}