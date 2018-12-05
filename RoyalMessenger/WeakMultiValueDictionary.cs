using Nito.AsyncEx;
using RoyalMessenger.Contracts;
using RoyalMessenger.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalMessenger
{
    /// <summary>A thread-safe asynchronous dictionary that can hold multiple weak references to values per key.</summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    internal sealed class WeakMultiValueDictionary<TKey, TItem> where TItem : class
    {
        private static readonly Logger Log = new Logger(typeof(WeakMultiValueDictionary<,>));

        private readonly IDictionary<TKey, ICollection<WeakReference<TItem>>> items = new Dictionary<TKey, ICollection<WeakReference<TItem>>>();
        private readonly AsyncLock itemsLock = new AsyncLock();

        /// <summary>Adds a new item to this dictionary.</summary>
        /// <param name="key">The key of the item.</param>
        /// <param name="item">The item to add.</param>
        public async Task AddAsync([Nullable] TKey key, TItem item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            using (await itemsLock.LockAsync().ConfigureAwait(false))
            {
                if (!items.ContainsKey(key))
                    items[key] = new List<WeakReference<TItem>>();

                items[key].Add(new WeakReference<TItem>(item));
            }
        }

        /// <summary>Removes an item from this dictionary. If the item is not in the dictionary, this does nothing.</summary>
        /// <param name="key">The key of the item.</param>
        /// <param name="item">The item to remove.</param>
        public async Task RemoveAsync([Nullable] TKey key, TItem item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            using (await itemsLock.LockAsync().ConfigureAwait(false))
            {
                if (!items.TryGetValue(key, out var values)) return;

                var references = values.Where(r => r.GetOrNull() == item).ToList();
                foreach (var reference in references) values.Remove(reference);

                if (values.Count == 0) items.Remove(key);
            }
        }

        /// <summary>
        /// Gets all valid items stored for a certain key, while also removing garbage-collected items.
        /// This method does not lock the dictionary and modifies it, so care must be taken to both
        /// lock the dictionary and make a copy of any LINQ iterators before calling this.
        /// </summary>
        /// <param name="key">The key to get the items for.</param>
        /// <returns>
        /// The valid non-garbage-collected items of the specified key,
        /// or an empty collection if the key is not in the dictionary.
        /// </returns>
        private IReadOnlyCollection<TItem> GetPurged([Nullable] TKey key)
        {
            if (!items.TryGetValue(key, out var references))
                return Array.Empty<TItem>();

            var result = new List<TItem>();
            var toRemove = new List<WeakReference<TItem>>();
            foreach (var reference in references)
            {
                if (reference.TryGetTarget(out var item))
                    result.Add(item);
                else
                    toRemove.Add(reference);
            }

            foreach (var item in toRemove)
                references.Remove(item);

            if (toRemove.Count > 0)
                Log.Info($"Purged {toRemove.Count} garbage-collected references from {key}");

            if (references.Count == 0)
            {
                Log.Trace($"{key} has no items left, removing it entirely");
                items.Remove(key);
            }

            return result;
        }

        /// <summary>
        /// Gets all items associated with a certain key.
        /// This should be used in favor of <see cref="GetMatchingAsync(Func{TKey, bool})"/> when the key is
        /// singular and known, as accessing a known key is faster than checking every key in the dictionary.
        /// </summary>
        /// <param name="key">The key to get the items of.</param>
        /// <returns>All valid, non-garbage-collected items stored for the specified key.</returns>
        public async Task<IReadOnlyCollection<TItem>> GetAsync([Nullable] TKey key)
        {
            using (await itemsLock.LockAsync().ConfigureAwait(false))
                return GetPurged(key);
        }

        /// <summary>Gets all non-garbage-collected items associated with keys that match a certain predicate.</summary>
        /// <param name="predicate">The predicate that determines if the items of a key should be returned.</param>
        /// <returns>All non-garbage-collected items associated with the keys that match the specified predicate.</returns>
        public async Task<IReadOnlyCollection<TItem>> GetMatchingAsync(Func<TKey, bool> predicate)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            using (await itemsLock.LockAsync().ConfigureAwait(false))
            {
                return items.Keys
                    .Where(predicate)
                    .ToList() // The GetPurged method modifies the dictionary, use a ToList copy to avoid invalidating our iterator
                    .SelectMany(GetPurged)
                    .ToList();
            }
        }
    }
}
