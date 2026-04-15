using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class LRUBrushCacheTests : BaseTestFixture
	{
		[Fact]
		public void GetReturnsBrushWithCorrectColor()
		{
			var cache = new LRUBrushCache(5);
			var brush = cache.Get(Colors.Red);

			Assert.NotNull(brush);
			Assert.Equal(Colors.Red, brush.Color);
		}

		[Fact]
		public void GetReturnsSameInstanceForSameColor()
		{
			var cache = new LRUBrushCache(5);
			var first = cache.Get(Colors.Blue);
			var second = cache.Get(Colors.Blue);

			Assert.Same(first, second);
		}

		[Fact]
		public void GetReturnsDifferentInstancesForDifferentColors()
		{
			var cache = new LRUBrushCache(5);
			var red = cache.Get(Colors.Red);
			var blue = cache.Get(Colors.Blue);

			Assert.NotSame(red, blue);
		}

		[Fact]
		public void CacheEvictsLeastRecentlyUsedWhenAtCapacity()
		{
			var cache = new LRUBrushCache(2);

			var red = cache.Get(Colors.Red);
			var blue = cache.Get(Colors.Blue);

			// Adding a third entry should evict the LRU entry (Red)
			cache.Get(Colors.Green);

			// Red should have been evicted — a new brush instance is created
			var redAfterEviction = cache.Get(Colors.Red);
			Assert.NotSame(red, redAfterEviction);
		}

		[Fact]
		public void AccessingEntryPromotesItAndPreventsEviction()
		{
			var cache = new LRUBrushCache(2);

			var red = cache.Get(Colors.Red);
			var blue = cache.Get(Colors.Blue);

			// Re-access Red to make it the most recently used
			cache.Get(Colors.Red);

			// Adding Green should evict Blue (LRU), not Red
			cache.Get(Colors.Green);

			var redAfterEviction = cache.Get(Colors.Red);
			Assert.Same(red, redAfterEviction);
		}

		[Fact]
		public void CacheWithCapacityOneEvictsPreviousOnEachInsert()
		{
			var cache = new LRUBrushCache(1);

			var red = cache.Get(Colors.Red);
			var blue = cache.Get(Colors.Blue);

			// Red should have been evicted
			var redAgain = cache.Get(Colors.Red);
			Assert.NotSame(red, redAgain);

			// Blue should have been evicted when Red was re-inserted
			var blueAgain = cache.Get(Colors.Blue);
			Assert.NotSame(blue, blueAgain);
		}

		[Fact]
		public void CacheDoesNotExceedCapacity()
		{
			int capacity = 3;
			var cache = new LRUBrushCache(capacity);

			var colors = new[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Yellow, Colors.Purple };

			foreach (var color in colors)
				cache.Get(color);

			// The last `capacity` colors should still be cached (same instance returned)
			var green = cache.Get(Colors.Green);
			var yellow = cache.Get(Colors.Yellow);
			var purple = cache.Get(Colors.Purple);

			Assert.Same(green, cache.Get(Colors.Green));
			Assert.Same(yellow, cache.Get(Colors.Yellow));
			Assert.Same(purple, cache.Get(Colors.Purple));
		}
	}
}
