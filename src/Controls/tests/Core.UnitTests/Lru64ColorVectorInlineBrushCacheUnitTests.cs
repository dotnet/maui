using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class Lru64ColorVectorInlineBrushCacheUnitTests : BaseTestFixture
	{
		// Builds a color whose ToUint() is 0xFF000000 | index, guaranteeing a distinct, stable key per index.
		static Color DistinctColor(int index) => Color.FromUint(0xFF000000u | (uint)index);

		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		[InlineData(65)]
		public void CtorThrowsWhenCapacityIsOutOfRange(int capacity)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new Lru64ColorVectorInlineBrushCache(capacity));
		}

		[Fact]
		public void ReturnsSameBrushInstanceForSameColor()
		{
			var cache = new Lru64ColorVectorInlineBrushCache(5);

			var first = cache.Get(Colors.Red);
			var second = cache.Get(Colors.Red);

			Assert.Same(first, second);
		}

		[Fact]
		public void ReturnsDifferentBrushInstancesForDifferentColors()
		{
			var cache = new Lru64ColorVectorInlineBrushCache(5);

			var red = cache.Get(Colors.Red);
			var green = cache.Get(Colors.Green);

			Assert.NotSame(red, green);
		}

		[Fact]
		public void ReturnedBrushHasRequestedColor()
		{
			var cache = new Lru64ColorVectorInlineBrushCache(5);

			var brush = cache.Get(Colors.Purple);

			Assert.Equal(Colors.Purple, brush.Color);
		}

		[Fact]
		public void EvictsLeastRecentlyUsedEntryWhenFull()
		{
			var cache = new Lru64ColorVectorInlineBrushCache(2);

			var red = cache.Get(Colors.Red);
			var green = cache.Get(Colors.Green);

			// Cache is full (Red, Green). Adding Blue must evict the LRU entry (Red).
			var blue = cache.Get(Colors.Blue);

			Assert.Same(green, cache.Get(Colors.Green));
			Assert.Same(blue, cache.Get(Colors.Blue));
			Assert.NotSame(red, cache.Get(Colors.Red));
		}

		[Fact]
		public void RecentlyUsedColorSurvivesEviction()
		{
			var cache = new Lru64ColorVectorInlineBrushCache(2);

			var red = cache.Get(Colors.Red);
			cache.Get(Colors.Green);

			// Touch Red so Green becomes the least-recently-used entry.
			cache.Get(Colors.Red);

			// Adding Blue must now evict Green, not Red.
			cache.Get(Colors.Blue);

			Assert.Same(red, cache.Get(Colors.Red));
			Assert.NotSame(cache.Get(Colors.Green), red);
		}

		[Fact]
		public void RetainsIdentityForAllColorsUpToCapacity()
		{
			const int capacity = 40;
			var cache = new Lru64ColorVectorInlineBrushCache(capacity);

			var brushes = new ImmutableBrush[capacity];
			for (int i = 0; i < capacity; i++)
			{
				brushes[i] = cache.Get(DistinctColor(i));
			}

			// Nothing was evicted, so every color must still map to its original brush instance.
			for (int i = 0; i < capacity; i++)
			{
				Assert.Same(brushes[i], cache.Get(DistinctColor(i)));
			}
		}

		[Fact]
		public void SupportsMaximumCapacityOf64()
		{
			const int capacity = 64;
			var cache = new Lru64ColorVectorInlineBrushCache(capacity);

			var first = cache.Get(DistinctColor(0));
			for (int i = 1; i < capacity; i++)
			{
				cache.Get(DistinctColor(i));
			}

			// Color 0 is the least-recently-used; the cache is exactly full and it should still be present.
			Assert.Same(first, cache.Get(DistinctColor(0)));
		}

		[Fact]
		public void ConcurrentAccessReturnsStableBrushIdentity()
		{
			const int distinctColors = 40; // under capacity, so no eviction races
			var cache = new Lru64ColorVectorInlineBrushCache(51);
			var expected = new ImmutableBrush[distinctColors];

			for (int i = 0; i < distinctColors; i++)
			{
				expected[i] = cache.Get(DistinctColor(i));
			}

			Parallel.For(0, 50_000, i =>
			{
				int colorIndex = i % distinctColors;
				var brush = cache.Get(DistinctColor(colorIndex));
				Assert.Same(expected[colorIndex], brush);
			});
		}
	}

	public class Lru64ColorVectorInlineUnitTests : BaseTestFixture
	{
		static Color DistinctColor(int index) => Color.FromUint(0xFF000000u | (uint)index);

		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		[InlineData(65)]
		public void CtorThrowsWhenCapacityIsOutOfRange(int capacity)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new Lru64ColorVectorInline<int>(capacity));
		}

		[Fact]
		public void GetOrAddInvokesFactoryOnlyOnMiss()
		{
			var cache = new Lru64ColorVectorInline<int>(4);
			int factoryCalls = 0;
			int Factory(Color c) { factoryCalls++; return (int)c.ToUint(); }

			var first = cache.GetOrAdd(Colors.Red, Factory);
			var second = cache.GetOrAdd(Colors.Red, Factory);

			Assert.Equal(first, second);
			Assert.Equal(1, factoryCalls);
		}

		[Fact]
		public void CountTracksInsertsAndSaturatesAtCapacity()
		{
			const int capacity = 5;
			var cache = new Lru64ColorVectorInline<int>(capacity);

			for (int i = 0; i < 20; i++)
			{
				cache.GetOrAdd(DistinctColor(i), c => (int)c.ToUint());
				Assert.Equal(Math.Min(i + 1, capacity), cache.Count);
			}
		}

		[Fact]
		public void ContainsKeyReflectsPresenceAndEviction()
		{
			var cache = new Lru64ColorVectorInline<int>(2);

			cache.GetOrAdd(DistinctColor(0), c => 0);
			cache.GetOrAdd(DistinctColor(1), c => 1);

			Assert.True(cache.ContainsKey(DistinctColor(0)));
			Assert.True(cache.ContainsKey(DistinctColor(1)));

			// Inserting a third color evicts the least-recently-used (color 0).
			cache.GetOrAdd(DistinctColor(2), c => 2);

			Assert.False(cache.ContainsKey(DistinctColor(0)));
			Assert.True(cache.ContainsKey(DistinctColor(1)));
			Assert.True(cache.ContainsKey(DistinctColor(2)));
		}

		[Fact]
		public void MaintainsInvariantsUnderChurn()
		{
			const int capacity = 8;
			var cache = new Lru64ColorVectorInline<int>(capacity);

			// Access a mix of repeated and new colors to exercise move-to-head, insert, and eviction.
			for (int i = 0; i < 500; i++)
			{
				int key = (i * 7) % 25; // 25 distinct colors churning through an 8-slot cache
				cache.GetOrAdd(DistinctColor(key), c => (int)c.ToUint());
				cache.AssertInvariants();
			}

			Assert.Equal(capacity, cache.Count);
		}

		[Fact]
		public void EvictsLeastRecentlyUsedAcrossSimdBoundary()
		{
			// A capacity larger than the SIMD width exercises both the vectorized scan and its scalar tail.
			const int capacity = 40;
			var cache = new Lru64ColorVectorInline<int>(capacity);

			for (int i = 0; i < capacity; i++)
			{
				cache.GetOrAdd(DistinctColor(i), c => (int)c.ToUint());
			}

			// Cache is full; color 0 is the LRU entry. A new color evicts it and nothing else.
			cache.GetOrAdd(DistinctColor(capacity), c => (int)c.ToUint());
			cache.AssertInvariants();

			Assert.False(cache.ContainsKey(DistinctColor(0)));
			Assert.True(cache.ContainsKey(DistinctColor(capacity)));
			for (int i = 1; i < capacity; i++)
			{
				Assert.True(cache.ContainsKey(DistinctColor(i)));
			}
		}
	}
}
