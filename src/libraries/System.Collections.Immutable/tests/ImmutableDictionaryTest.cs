// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit;

namespace System.Collections.Immutable.Tests
{
    public partial class ImmutableDictionaryTest : ImmutableDictionaryTestBase
    {
        [Fact]
        public void AddExistingKeySameValueTest()
        {
            AddExistingKeySameValueTestHelper(Empty(StringComparer.Ordinal, StringComparer.Ordinal), "Company", "Microsoft", "Microsoft");
            AddExistingKeySameValueTestHelper(Empty(StringComparer.Ordinal, StringComparer.OrdinalIgnoreCase), "Company", "Microsoft", "MICROSOFT");
        }

        [Fact]
        public void AddExistingKeyDifferentValueTest()
        {
            AddExistingKeyDifferentValueTestHelper(Empty(StringComparer.Ordinal, StringComparer.Ordinal), "Company", "Microsoft", "MICROSOFT");
        }

        [Fact]
        public void AddRangeShouldThrowOnNullKeyTest()
        {
            var items = new[] { new KeyValuePair<string, string>(null, "value") };

            ImmutableDictionary<string, string> map = Empty(StringComparer.Ordinal, StringComparer.Ordinal);
            Assert.Throws<ArgumentNullException>(() => map.AddRange(items));

            map = map.WithComparers(new BadHasher<string>());
            Assert.Throws<ArgumentNullException>(() => map.AddRange(items));
        }

        [Fact]
        public void UnorderedChangeTest()
        {
            ImmutableDictionary<string, string> map = Empty<string, string>(StringComparer.Ordinal)
                .Add("Johnny", "Appleseed")
                .Add("JOHNNY", "Appleseed");
            Assert.Equal(2, map.Count);
            Assert.True(map.ContainsKey("Johnny"));
            Assert.False(map.ContainsKey("johnny"));
            ImmutableDictionary<string, string> newMap = map.WithComparers(StringComparer.OrdinalIgnoreCase);
            Assert.Equal(1, newMap.Count);
            Assert.True(newMap.ContainsKey("Johnny"));
            Assert.True(newMap.ContainsKey("johnny")); // because it's case insensitive
        }

        [Fact]
        public void ToSortedTest()
        {
            ImmutableDictionary<string, string> map = Empty<string, string>(StringComparer.Ordinal)
                .Add("Johnny", "Appleseed")
                .Add("JOHNNY", "Appleseed");
            ImmutableSortedDictionary<string, string> sortedMap = map.ToImmutableSortedDictionary(StringComparer.Ordinal);
            Assert.Equal(sortedMap.Count, map.Count);
            CollectionAssertAreEquivalent<KeyValuePair<string, string>>(sortedMap.ToList(), map.ToList());
        }

        [Fact]
        public void SetItemUpdateEqualKeyTest()
        {
            ImmutableDictionary<string, int> map = Empty<string, int>().WithComparers(StringComparer.OrdinalIgnoreCase)
                .SetItem("A", 1);
            map = map.SetItem("a", 2);
            Assert.Equal("a", map.Keys.Single());
        }

        [Fact]
        public void SetItemsThrowOnNullKey()
        {
            ImmutableDictionary<string, int> map = Empty<string, int>().WithComparers(StringComparer.OrdinalIgnoreCase);
            KeyValuePair<string, int>[] items = new[] { new KeyValuePair<string, int>(null, 0) };
            Assert.Throws<ArgumentNullException>(() => map.SetItems(items));
            map = map.WithComparers(new BadHasher<string>());
            Assert.Throws<ArgumentNullException>(() => map.SetItems(items));
        }

        /// <summary>
        /// Verifies that the specified value comparer is applied when
        /// checking for equality.
        /// </summary>
        [Fact]
        public void SetItemUpdateEqualKeyWithValueEqualityByComparer()
        {
            ImmutableDictionary<string, CaseInsensitiveString> map = Empty<string, CaseInsensitiveString>().WithComparers(StringComparer.OrdinalIgnoreCase, new MyStringOrdinalComparer());
            string key = "key";
            string value1 = "Hello";
            string value2 = "hello";
            map = map.SetItem(key, new CaseInsensitiveString(value1));
            map = map.SetItem(key, new CaseInsensitiveString(value2));
            Assert.Equal(value2, map[key].Value);

            Assert.Same(map, map.SetItem(key, new CaseInsensitiveString(value2)));
        }

        [Fact]
        public void ContainsValueTest()
        {
            this.ContainsValueTestHelper(ImmutableDictionary<int, GenericParameterHelper>.Empty, 1, new GenericParameterHelper());
        }

        [Fact]
        public void ContainsValue_NoSuchValue_ReturnsFalse()
        {
            ImmutableDictionary<int, string> dictionary = new Dictionary<int, string>
            {
                { 1, "a" },
                { 2, "b" }
            }.ToImmutableDictionary();
            Assert.False(dictionary.ContainsValue("c"));
            Assert.False(dictionary.ContainsValue(null));
        }

        [Fact]
        public void Create()
        {
            IEnumerable<KeyValuePair<string, string>> pairs = new Dictionary<string, string> { { "a", "b" } };
            ReadOnlySpan<KeyValuePair<string, string>> pairsWithDuplicates =
            [
                new KeyValuePair<string, string>("a", "b"),
                new KeyValuePair<string, string>("a", "c"),
                new KeyValuePair<string, string>("b", "d"),
                new KeyValuePair<string, string>("B", "e"),
                new KeyValuePair<string, string>("a", "f"),
            ];
            StringComparer keyComparer = StringComparer.OrdinalIgnoreCase;
            StringComparer valueComparer = StringComparer.CurrentCulture;

            ImmutableDictionary<string, string> dictionary = ImmutableDictionary.Create<string, string>();
            Assert.Equal(0, dictionary.Count);
            Assert.Same(EqualityComparer<string>.Default, dictionary.KeyComparer);
            Assert.Same(EqualityComparer<string>.Default, dictionary.ValueComparer);

            dictionary = ImmutableDictionary.Create<string, string>(keyComparer);
            Assert.Equal(0, dictionary.Count);
            Assert.Same(keyComparer, dictionary.KeyComparer);
            Assert.Same(EqualityComparer<string>.Default, dictionary.ValueComparer);

            dictionary = ImmutableDictionary.Create(keyComparer, valueComparer);
            Assert.Equal(0, dictionary.Count);
            Assert.Same(keyComparer, dictionary.KeyComparer);
            Assert.Same(valueComparer, dictionary.ValueComparer);

            dictionary = ImmutableDictionary.CreateRange(pairs);
            Assert.Equal(1, dictionary.Count);
            Assert.Same(EqualityComparer<string>.Default, dictionary.KeyComparer);
            Assert.Same(EqualityComparer<string>.Default, dictionary.ValueComparer);

            dictionary = ImmutableDictionary.CreateRange(keyComparer, pairs);
            Assert.Equal(1, dictionary.Count);
            Assert.Same(keyComparer, dictionary.KeyComparer);
            Assert.Same(EqualityComparer<string>.Default, dictionary.ValueComparer);

            dictionary = ImmutableDictionary.CreateRange(keyComparer, valueComparer, pairs);
            Assert.Equal(1, dictionary.Count);
            Assert.Same(keyComparer, dictionary.KeyComparer);
            Assert.Same(valueComparer, dictionary.ValueComparer);

            dictionary = ImmutableDictionary.CreateRangeWithOverwrite<string, string>();
            Assert.Equal(0, dictionary.Count);
            Assert.Same(EqualityComparer<string>.Default, dictionary.KeyComparer);
            Assert.Same(EqualityComparer<string>.Default, dictionary.ValueComparer);

            dictionary = ImmutableDictionary.CreateRangeWithOverwrite<string, string>(keyComparer);
            Assert.Equal(0, dictionary.Count);
            Assert.Same(keyComparer, dictionary.KeyComparer);
            Assert.Same(EqualityComparer<string>.Default, dictionary.ValueComparer);

            dictionary = ImmutableDictionary.CreateRangeWithOverwrite(pairsWithDuplicates);
            Assert.Equal(3, dictionary.Count);
            Assert.Same(EqualityComparer<string>.Default, dictionary.KeyComparer);
            Assert.Same(EqualityComparer<string>.Default, dictionary.ValueComparer);
            Assert.Equal("f", dictionary["a"]);
            Assert.Equal("d", dictionary["b"]);
            Assert.Equal("e", dictionary["B"]);

            dictionary = ImmutableDictionary.CreateRangeWithOverwrite(keyComparer, pairsWithDuplicates);
            Assert.Equal(2, dictionary.Count);
            Assert.Same(keyComparer, dictionary.KeyComparer);
            Assert.Same(EqualityComparer<string>.Default, dictionary.ValueComparer);
            Assert.Equal("f", dictionary["a"]);
            Assert.Equal("e", dictionary["b"]);
        }

        [Fact]
        public void ToImmutableDictionary()
        {
            IEnumerable<KeyValuePair<string, string>> pairs = new Dictionary<string, string> { { "a", "B" } };
            StringComparer keyComparer = StringComparer.OrdinalIgnoreCase;
            StringComparer valueComparer = StringComparer.CurrentCulture;

            ImmutableDictionary<string, string> dictionary = pairs.ToImmutableDictionary();
            Assert.Equal(1, dictionary.Count);
            Assert.Same(EqualityComparer<string>.Default, dictionary.KeyComparer);
            Assert.Same(EqualityComparer<string>.Default, dictionary.ValueComparer);

            dictionary = pairs.ToImmutableDictionary(keyComparer);
            Assert.Equal(1, dictionary.Count);
            Assert.Same(keyComparer, dictionary.KeyComparer);
            Assert.Same(EqualityComparer<string>.Default, dictionary.ValueComparer);

            dictionary = pairs.ToImmutableDictionary(keyComparer, valueComparer);
            Assert.Equal(1, dictionary.Count);
            Assert.Same(keyComparer, dictionary.KeyComparer);
            Assert.Same(valueComparer, dictionary.ValueComparer);

            dictionary = pairs.ToImmutableDictionary(p => p.Key.ToUpperInvariant(), p => p.Value.ToLowerInvariant());
            Assert.Equal(1, dictionary.Count);
            Assert.Equal("A", dictionary.Keys.Single());
            Assert.Equal("b", dictionary.Values.Single());
            Assert.Same(EqualityComparer<string>.Default, dictionary.KeyComparer);
            Assert.Same(EqualityComparer<string>.Default, dictionary.ValueComparer);

            dictionary = pairs.ToImmutableDictionary(p => p.Key.ToUpperInvariant(), p => p.Value.ToLowerInvariant(), keyComparer);
            Assert.Equal(1, dictionary.Count);
            Assert.Equal("A", dictionary.Keys.Single());
            Assert.Equal("b", dictionary.Values.Single());
            Assert.Same(keyComparer, dictionary.KeyComparer);
            Assert.Same(EqualityComparer<string>.Default, dictionary.ValueComparer);

            dictionary = pairs.ToImmutableDictionary(p => p.Key.ToUpperInvariant(), p => p.Value.ToLowerInvariant(), keyComparer, valueComparer);
            Assert.Equal(1, dictionary.Count);
            Assert.Equal("A", dictionary.Keys.Single());
            Assert.Equal("b", dictionary.Values.Single());
            Assert.Same(keyComparer, dictionary.KeyComparer);
            Assert.Same(valueComparer, dictionary.ValueComparer);

            var list = new int[] { 1, 2 };
            ImmutableDictionary<double, int> intDictionary = list.ToImmutableDictionary(n => (double)n);
            Assert.Equal(1, intDictionary[1.0]);
            Assert.Equal(2, intDictionary[2.0]);
            Assert.Equal(2, intDictionary.Count);

            ImmutableDictionary<string, int> stringIntDictionary = list.ToImmutableDictionary(n => n.ToString(), StringComparer.OrdinalIgnoreCase);
            Assert.Same(StringComparer.OrdinalIgnoreCase, stringIntDictionary.KeyComparer);
            Assert.Equal(1, stringIntDictionary["1"]);
            Assert.Equal(2, stringIntDictionary["2"]);
            Assert.Equal(2, intDictionary.Count);

            AssertExtensions.Throws<ArgumentNullException>("keySelector", () => list.ToImmutableDictionary<int, int>(null));
            AssertExtensions.Throws<ArgumentNullException>("keySelector", () => list.ToImmutableDictionary<int, int, int>(null, v => v));
            AssertExtensions.Throws<ArgumentNullException>("elementSelector", () => list.ToImmutableDictionary<int, int, int>(k => k, null));

            list.ToDictionary(k => k, v => v, null); // verifies BCL behavior is to not throw.
            list.ToImmutableDictionary(k => k, v => v, null, null);
        }

        [Fact]
        public void ToImmutableDictionaryOptimized()
        {
            ImmutableDictionary<string, string> dictionary = ImmutableDictionary.Create<string, string>();
            ImmutableDictionary<string, string> result = dictionary.ToImmutableDictionary();
            Assert.Same(dictionary, result);

            StringComparer cultureComparer = StringComparer.CurrentCulture;
            result = dictionary.WithComparers(cultureComparer, StringComparer.OrdinalIgnoreCase);
            Assert.Same(cultureComparer, result.KeyComparer);
            Assert.Same(StringComparer.OrdinalIgnoreCase, result.ValueComparer);
        }

        [Fact]
        public void WithComparers()
        {
            ImmutableDictionary<string, string> map = ImmutableDictionary.Create<string, string>().Add("a", "1").Add("B", "1");
            Assert.Same(EqualityComparer<string>.Default, map.KeyComparer);
            Assert.True(map.ContainsKey("a"));
            Assert.False(map.ContainsKey("A"));

            map = map.WithComparers(StringComparer.OrdinalIgnoreCase);
            Assert.Same(StringComparer.OrdinalIgnoreCase, map.KeyComparer);
            Assert.Equal(2, map.Count);
            Assert.True(map.ContainsKey("a"));
            Assert.True(map.ContainsKey("A"));
            Assert.True(map.ContainsKey("b"));

            StringComparer cultureComparer = StringComparer.CurrentCulture;
            map = map.WithComparers(StringComparer.OrdinalIgnoreCase, cultureComparer);
            Assert.Same(StringComparer.OrdinalIgnoreCase, map.KeyComparer);
            Assert.Same(cultureComparer, map.ValueComparer);
            Assert.Equal(2, map.Count);
            Assert.True(map.ContainsKey("a"));
            Assert.True(map.ContainsKey("A"));
            Assert.True(map.ContainsKey("b"));
        }

        [Fact]
        public void WithComparersCollisions()
        {
            // First check where collisions have matching values.
            ImmutableDictionary<string, string> map = ImmutableDictionary.Create<string, string>()
                .Add("a", "1").Add("A", "1");
            map = map.WithComparers(StringComparer.OrdinalIgnoreCase);
            Assert.Same(StringComparer.OrdinalIgnoreCase, map.KeyComparer);
            Assert.Equal(1, map.Count);
            Assert.True(map.ContainsKey("a"));
            Assert.Equal("1", map["a"]);

            // Now check where collisions have conflicting values.
            map = ImmutableDictionary.Create<string, string>()
              .Add("a", "1").Add("A", "2").Add("b", "3");
            AssertExtensions.Throws<ArgumentException>(null, () => map.WithComparers(StringComparer.OrdinalIgnoreCase));

            // Force all values to be considered equal.
            map = map.WithComparers(StringComparer.OrdinalIgnoreCase, EverythingEqual<string>.Default);
            Assert.Same(StringComparer.OrdinalIgnoreCase, map.KeyComparer);
            Assert.Same(EverythingEqual<string>.Default, map.ValueComparer);
            Assert.Equal(2, map.Count);
            Assert.True(map.ContainsKey("a"));
            Assert.True(map.ContainsKey("b"));
        }

        [Fact]
        public void CollisionExceptionMessageContainsKey()
        {
            ImmutableDictionary<string, string> map = ImmutableDictionary.Create<string, string>()
                .Add("firstKey", "1").Add("secondKey", "2");
            ArgumentException exception = AssertExtensions.Throws<ArgumentException>(null, () => map.Add("firstKey", "3"));
            Assert.Contains("firstKey", exception.Message);
        }

        [Fact]
        public void WithComparersEmptyCollection()
        {
            ImmutableDictionary<string, string> map = ImmutableDictionary.Create<string, string>();
            Assert.Same(EqualityComparer<string>.Default, map.KeyComparer);
            map = map.WithComparers(StringComparer.OrdinalIgnoreCase);
            Assert.Same(StringComparer.OrdinalIgnoreCase, map.KeyComparer);
        }

        [Fact]
        public void GetValueOrDefaultOfIImmutableDictionary()
        {
            IImmutableDictionary<string, int> empty = ImmutableDictionary.Create<string, int>();
            IImmutableDictionary<string, int> populated = ImmutableDictionary.Create<string, int>().Add("a", 5);
            Assert.Equal(0, empty.GetValueOrDefault("a"));
            Assert.Equal(1, empty.GetValueOrDefault("a", 1));
            Assert.Equal(5, populated.GetValueOrDefault("a"));
            Assert.Equal(5, populated.GetValueOrDefault("a", 1));
        }

        [Fact]
        public void GetValueOrDefaultOfConcreteType()
        {
            ImmutableDictionary<string, int> empty = ImmutableDictionary.Create<string, int>();
            ImmutableDictionary<string, int> populated = ImmutableDictionary.Create<string, int>().Add("a", 5);
            Assert.Equal(0, empty.GetValueOrDefault("a"));
            Assert.Equal(1, empty.GetValueOrDefault("a", 1));
            Assert.Equal(5, populated.GetValueOrDefault("a"));
            Assert.Equal(5, populated.GetValueOrDefault("a", 1));
        }

        [Fact]
        public void EnumeratorRecyclingMisuse()
        {
            ImmutableDictionary<int, int> collection = ImmutableDictionary.Create<int, int>().Add(5, 3);
            ImmutableDictionary<int, int>.Enumerator enumerator = collection.GetEnumerator();
            ImmutableDictionary<int, int>.Enumerator enumeratorCopy = enumerator;
            Assert.True(enumerator.MoveNext());
            Assert.False(enumerator.MoveNext());
            enumerator.Dispose();
            Assert.Throws<ObjectDisposedException>(() => enumerator.MoveNext());
            Assert.Throws<ObjectDisposedException>(() => enumerator.Reset());
            Assert.Throws<ObjectDisposedException>(() => enumerator.Current);
            Assert.Throws<ObjectDisposedException>(() => enumeratorCopy.MoveNext());
            Assert.Throws<ObjectDisposedException>(() => enumeratorCopy.Reset());
            Assert.Throws<ObjectDisposedException>(() => enumeratorCopy.Current);
            enumerator.Dispose(); // double-disposal should not throw
            enumeratorCopy.Dispose();

            // We expect that acquiring a new enumerator will use the same underlying Stack<T> object,
            // but that it will not throw exceptions for the new enumerator.
            enumerator = collection.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.False(enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
            enumerator.Dispose();
        }

        [Fact]
        public void Clear_NoComparer_ReturnsEmptyWithoutComparer()
        {
            ImmutableDictionary<string, int> dictionary = new Dictionary<string, int>
            {
                { "a", 1 }
            }.ToImmutableDictionary();
            Assert.Same(ImmutableDictionary<string, int>.Empty, dictionary.Clear());
            Assert.NotEmpty(dictionary);
        }

        [Fact]
        public void Clear_HasComparer_ReturnsEmptyWithOriginalComparer()
        {
            ImmutableDictionary<string, int> dictionary = new Dictionary<string, int>
            {
                { "a", 1 }
            }.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

            ImmutableDictionary<string, int> clearedDictionary = dictionary.Clear();
            Assert.NotSame(ImmutableDictionary<string, int>.Empty, clearedDictionary.Clear());
            Assert.NotEmpty(dictionary);

            clearedDictionary = clearedDictionary.Add("a", 1);
            Assert.True(clearedDictionary.ContainsKey("A"));
        }

        [Fact]
        public void Indexer_KeyNotFoundException_ContainsKeyInMessage()
        {
            ImmutableDictionary<string, string> map = ImmutableDictionary.Create<string, string>()
                .Add("a", "1").Add("b", "2");
            KeyNotFoundException exception = Assert.Throws<KeyNotFoundException>(() => map["c"]);
            Assert.Contains("'c'", exception.Message);
        }

        protected override IImmutableDictionary<TKey, TValue> Empty<TKey, TValue>()
        {
            return ImmutableDictionaryTest.Empty<TKey, TValue>();
        }

        protected override IImmutableDictionary<string, TValue> Empty<TValue>(StringComparer comparer)
        {
            return ImmutableDictionary.Create<string, TValue>(comparer);
        }

        protected override IEqualityComparer<TValue> GetValueComparer<TKey, TValue>(IImmutableDictionary<TKey, TValue> dictionary)
        {
            return ((ImmutableDictionary<TKey, TValue>)dictionary).ValueComparer;
        }

        protected void ContainsValueTestHelper<TKey, TValue>(ImmutableDictionary<TKey, TValue> map, TKey key, TValue value)
        {
            Assert.False(map.ContainsValue(value));
            Assert.True(map.Add(key, value).ContainsValue(value));
        }

        private static ImmutableDictionary<TKey, TValue> Empty<TKey, TValue>(IEqualityComparer<TKey> keyComparer = null, IEqualityComparer<TValue> valueComparer = null)
        {
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer);
        }

        /// <summary>
        /// An ordinal comparer for case-insensitive strings.
        /// </summary>
        private class MyStringOrdinalComparer : EqualityComparer<CaseInsensitiveString>
        {
            public override bool Equals(CaseInsensitiveString x, CaseInsensitiveString y)
            {
                return StringComparer.Ordinal.Equals(x.Value, y.Value);
            }

            public override int GetHashCode(CaseInsensitiveString obj)
            {
                return StringComparer.Ordinal.GetHashCode(obj.Value);
            }
        }

        /// <summary>
        /// A string-wrapper that considers equality based on case insensitivity.
        /// </summary>
        private class CaseInsensitiveString
        {
            public CaseInsensitiveString(string value)
            {
                Value = value;
            }

            public string Value { get; private set; }
            public override int GetHashCode()
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(this.Value);
            }
            public override bool Equals(object obj)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(this.Value, ((CaseInsensitiveString)obj).Value);
            }
        }
    }
}
