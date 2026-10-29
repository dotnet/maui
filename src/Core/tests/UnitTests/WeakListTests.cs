using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	public class WeakListTests
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference AddCollectableObject(WeakList<object> list)
		{
			var evict = new object();
			list.Add(evict);
			return new WeakReference(evict);
		}

		[Fact]
		public async Task ObjectsAreEvicted_GetEnumerator()
		{
			var expected = new object();
			var list = new WeakList<object> { expected };
			var weakReference = AddCollectableObject(list);
			list.CleanupThreshold = 1;

			Assert.False(await weakReference.WaitForCollect(), "Object failed to be collected");

			foreach (var item in list)
			{
				// Do nothing
			}

			Assert.Equal(1, list.Count);
			Assert.Equal(expected, list.First());
		}

		[Fact]
		public async Task ObjectsAreEvicted_Remove()
		{
			var expected = new object();
			var list = new WeakList<object> { expected };
			var weakReference = AddCollectableObject(list);
			list.CleanupThreshold = 1;

			Assert.False(await weakReference.WaitForCollect(), "Object failed to be collected");

			list.Remove(expected);

			Assert.Equal(0, list.Count);
		}

		[Fact]
		public async Task ObjectsAreEvicted_Add()
		{
			var expected = new object();
			var list = new WeakList<object> { expected };
			var weakReference = AddCollectableObject(list);
			list.CleanupThreshold = 1;

			Assert.False(await weakReference.WaitForCollect(), "Object failed to be collected");

			var triggerCleanup = new object();
			list.Add(triggerCleanup);

			Assert.Equal(2, list.Count);
			Assert.Equal(expected, list.First());
		}
	}
}
