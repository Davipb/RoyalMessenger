using System;

namespace RoyalMessenger
{
    internal static class WeakReferenceExtensions
    {
        /// <summary>Gets the target of a weak reference, or <see langword="null" /> if the target has already been garbage collected.</summary>
        /// <typeparam name="T">The type of the item reference by the weak reference.</typeparam>
        /// <param name="reference">The weak reference.</param>
        /// <returns>The item being referenced, or <see langword="null" /> if it has already been garbage collected.</returns>
        public static T GetOrNull<T>(this WeakReference<T> reference) where T : class
        {
            if (reference is null) throw new ArgumentNullException(nameof(reference));

            if (reference.TryGetTarget(out var target)) return target;
            return null;
        }
    }
}
