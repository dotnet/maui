using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	public class WeakListTests
	{
		[Fact]
		public async Task ObjectsAreEvicted_GetEnumerator()
		{
			var expected = new object();
			var list = new WeakList<object> { expected, new object() };
			list.CleanupThreshold = 1;

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

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
			var list = new WeakList<object> { expected, new object() };
			list.CleanupThreshold = 1;

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			list.Remove(expected);

			Assert.Equal(0, list.Count);
		}

		[Fact]
		public async Task ObjectsAreEvicted_Add()
		{
			var expected = new object();
			var list = new WeakList<object> { expected, new object() };
			list.CleanupThreshold = 1;

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			list.Add(new object());

			Assert.Equal(2, list.Count);
			Assert.Equal(expected, list.First());
		}
	}
}
