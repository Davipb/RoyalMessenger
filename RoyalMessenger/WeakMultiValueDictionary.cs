using Nito.AsyncEx;
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
        private readonly IDictionary<TKey, ICollection<WeakReference<TItem>>> items = new Dictionary<TKey, ICollection<WeakReference<TItem>>>();
        private readonly AsyncReaderWriterLock itemsLock = new AsyncReaderWriterLock();

        /// <summary>Removes all garbage-collected references from a collection.</summary>
        /// <param name="references">The collection to purge.</param>
        private static void Purge(ICollection<WeakReference<TItem>> references)
        {
            var toRemove = references.Where(r => !r.TryGetTarget(out _)).ToList();
            foreach (var item in toRemove) references.Remove(item);
        }

        /// <summary>Adds a new item to this dictionary.</summary>
        /// <param name="key">The key of the item.</param>
        /// <param name="item">The item to add.</param>
        public async Task AddAsync(TKey key, TItem item)
        {
            using (await itemsLock.WriterLockAsync().ConfigureAwait(false))
            {
                if (!items.ContainsKey(key))
                    items[key] = new List<WeakReference<TItem>>();

                items[key].Add(new WeakReference<TItem>(item));
            }
        }

        /// <summary>Removes an item from this dictionary. If the item is not in the dictionary, this does nothing.</summary>
        /// <param name="key">The key of the item.</param>
        /// <param name="item">The item to remove.</param>
        public async Task RemoveAsync(TKey key, TItem item)
        {
            using (await itemsLock.WriterLockAsync().ConfigureAwait(false))
            {
                if (!items.TryGetValue(key, out var values)) return;

                var references = values.Where(r => r.GetOrNull() == item).ToList();
                foreach (var reference in references) values.Remove(reference);

                if (values.Count == 0) items.Remove(key);
            }
        }

        /// <summary>Gets all values associated with a key that haven't been garbage-collected yet.</summary>
        /// <param name="key">The key to get the values of.</param>
        /// <returns>All items associated with the specified key that haven't been garbage-collected yet.</returns>
        public async Task<IReadOnlyCollection<TItem>> GetAsync(TKey key)
        {
            using (await itemsLock.ReaderLockAsync().ConfigureAwait(false))
            {
                if (!items.TryGetValue(key, out var values))
                    return Array.Empty<TItem>();

                Purge(values);

                return values
                    .Select(r => r.GetOrNull())
                    .Where(v => v != null)
                    .ToList()
                    .AsReadOnly();
            }
        }
    }
}
