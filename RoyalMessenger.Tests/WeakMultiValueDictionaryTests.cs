using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RoyalMessenger.Tests
{
    public class WeakMultiValueDictionaryTests
    {
        private class Item { }

        /// <summary>Generates a specified amount of dummy items.</summary>
        /// <param name="count">The number of items to generate.</param>
        private static List<Item> GenerateItems(int count)
        {
            var result = new List<Item>(count);
            for (var i = 0; i < count; i++)
                result.Add(new Item());

            return result;
        }


        [Fact]
        public async Task Add_SingleItem_KeepsItem()
        {
            var key = "good";
            var item = new Item();

            var dictionary = new WeakMultiValueDictionary<string, Item>();
            await dictionary.AddAsync(key, item);

            var result = await dictionary.GetAsync(key);
            Assert.Single(result, item);
        }

        [Fact]
        public async Task Add_MultipleItems_KeepsItems()
        {
            var key = "good";
            var expected = GenerateItems(4);

            var dictionary = new WeakMultiValueDictionary<string, Item>();
            foreach (var item in expected)
                await dictionary.AddAsync(key, item);

            var actual = await dictionary.GetAsync(key);
            TestHelper.AssertEquivalent(expected, actual);
        }

        [Fact]
        public async Task Add_RemoveItem_DoesntKeepItem()
        {
            var key = "good";
            var item = new Item();

            var dictionary = new WeakMultiValueDictionary<string, Item>();
            await dictionary.AddAsync(key, item);
            await dictionary.RemoveAsync(key, item);

            var result = await dictionary.GetAsync(key);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Add_MultipleItems_RemovingItems_KeepsOnlyNonRemovedItems()
        {
            var key = "good";
            var expected = GenerateItems(4);
            var removed = GenerateItems(4);

            var dictionary = new WeakMultiValueDictionary<string, Item>();

            foreach (var item in expected.Union(removed))
                await dictionary.AddAsync(key, item);

            foreach (var item in removed)
                await dictionary.RemoveAsync(key, item);

            var actual = await dictionary.GetAsync(key);
            TestHelper.AssertEquivalent(expected, actual);
        }

        [Fact]
        public async Task Add_SingleItem_GarbageCollected_RemovesItem()
        {
            var key = "good";

            var dictionary = new WeakMultiValueDictionary<string, Item>();
            await dictionary.AddAsync(key, new Item());

            await TestHelper.ForceGarbageCollectionAsync();

            var result = await dictionary.GetAsync(key);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Add_MultipleItems_GarbageCollected_RemovesItems()
        {
            var key = "good";
            var expected = GenerateItems(4);
            var collected = GenerateItems(4);

            var dictionary = new WeakMultiValueDictionary<string, Item>();

            foreach (var item in expected.Union(collected))
                await dictionary.AddAsync(key, item);

            collected = null;
            await TestHelper.ForceGarbageCollectionAsync();

            var actual = await dictionary.GetAsync(key);
            TestHelper.AssertEquivalent(expected, actual);
        }

        [Fact]
        public async Task GetMatching_FiltersKeys()
        {
            var goodItems1 = GenerateItems(4);
            var goodItems2 = GenerateItems(4);
            var expected = goodItems1.Union(goodItems2).ToList();

            var badItems = GenerateItems(4);

            var dictionary = new WeakMultiValueDictionary<string, Item>();

            foreach (var item in goodItems1)
                await dictionary.AddAsync("good1", item);

            foreach (var item in goodItems2)
                await dictionary.AddAsync("good2", item);

            foreach (var item in badItems)
                await dictionary.AddAsync("bad", item);

            var result = await dictionary.GetMatchingAsync(k => k.StartsWith("good"));
            TestHelper.AssertEquivalent(expected, result);
        }
    }
}
