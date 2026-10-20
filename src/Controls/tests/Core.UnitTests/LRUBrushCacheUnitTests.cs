using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class LRUBrushCacheUnitTests : BaseTestFixture
	{
		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public void LruBrushCacheCtorThrowsWhenCapacityIsNotPositive(int capacity)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new LRUBrushCache(capacity));
		}

		[Fact]
		public void LruBrushCacheReturnsSameBrushForSameColor()
		{
			var cache = new LRUBrushCache(5);

			var first = cache.Get(Colors.Red);
			var second = cache.Get(Colors.Red);

			Assert.Same(first, second);
		}

		[Fact]
		public void LruBrushCacheSeededCtorThrowsWhenBrushesAreNull()
		{
			Assert.Throws<ArgumentNullException>(() => new LRUBrushCache(1, null));
		}

		[Fact]
		public void LruBrushCacheSeededCtorThrowsWhenBrushCountExceedsCapacity()
		{
			var brushes = new Dictionary<Color, ImmutableBrush>
			{
				[Colors.Red] = new ImmutableBrush(Colors.Red),
				[Colors.Green] = new ImmutableBrush(Colors.Green),
			};

			Assert.Throws<ArgumentException>(() => new LRUBrushCache(1, brushes));
		}

		[Fact]
		public void LruBrushCacheSeededCtorReusesSeededBrushInstances()
		{
			var red = new ImmutableBrush(Colors.Red);
			var green = new ImmutableBrush(Colors.Green);

			var brushes = new Dictionary<Color, ImmutableBrush>
			{
				[Colors.Red] = red,
				[Colors.Green] = green,
			};

			var cache = new LRUBrushCache(2, brushes);

			Assert.Same(red, cache.Get(Colors.Red));
			Assert.Same(green, cache.Get(Colors.Green));
		}

		[Fact]
		public void LruBrushCacheEvictsLeastRecentlyUsedEntry()
		{
			var cache = new LRUBrushCache(2);

			var red = cache.Get(Colors.Red);
			var green = cache.Get(Colors.Green);

			cache.Get(Colors.Red);
			var blue = cache.Get(Colors.Blue);

			Assert.Same(red, cache.Get(Colors.Red));
			Assert.Same(blue, cache.Get(Colors.Blue));
			Assert.NotSame(green, cache.Get(Colors.Green));
		}
	}
}
