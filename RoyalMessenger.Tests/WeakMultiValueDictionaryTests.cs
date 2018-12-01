using System.Threading.Tasks;
using Xunit;

namespace RoyalMessenger.Tests
{
    public class WeakMultiValueDictionaryTests
    {
        private class Item
        {
            public int Value { get; }

            public Item(int value) => Value = value;
        }


        [Fact]
        public async Task Add_SingleItem_KeepsItem()
        {
            var key = "good";
            var item = new Item(42);

            var dictionary = new WeakMultiValueDictionary<string, Item>();
            await dictionary.AddAsync(key, item);

            var result = await dictionary.GetAsync(key);
            Assert.Single(result, item);
        }

        [Fact]
        public async Task Add_MultipleItems_KeepsItems()
        {
            var key = "good";
            var expected = new[] { new Item(42), new Item(43), new Item(44), new Item(1337) };

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
            var item = new Item(42);

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
            var all = new[] { new Item(42), new Item(43), new Item(44), new Item(1337) };
            var expected = new[] { all[0], all[2] };
            var removed = new[] { all[1], all[3] };

            var dictionary = new WeakMultiValueDictionary<string, Item>();

            foreach (var item in all)
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
            await dictionary.AddAsync(key, new Item(42));

            await TestHelper.ForceGarbageCollectionAsync();

            var result = await dictionary.GetAsync(key);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Add_MultipleItems_GarbageCollected_RemovesItems()
        {
            var key = "good";
            var all = new[] { new Item(42), new Item(43), new Item(44), new Item(1337) };
            var expected = new[] { all[0], all[2] };
            var collected = new[] { all[1], all[3] };

            var dictionary = new WeakMultiValueDictionary<string, Item>();

            foreach (var item in all)
                await dictionary.AddAsync(key, item);


            all = collected = null;
            await TestHelper.ForceGarbageCollectionAsync();

            var actual = await dictionary.GetAsync(key);
            TestHelper.AssertEquivalent(expected, actual);
        }
    }
}
