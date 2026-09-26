using Microsoft.Maui.Foldable;
using Xunit;

namespace Microsoft.Maui.Controls.Foldable.UnitTests
{
	public class FoldableMonitorRegistryTests
	{
		[Fact]
		public void ReusesMonitorForSameView()
		{
			var disposed = 0;
			using var registry = new FoldableMonitorRegistry<object, object>(_ => disposed++);
			var view = new object();
			var monitor = new object();

			var first = registry.GetOrAdd(view, () => monitor);
			var second = registry.GetOrAdd(view, () => new object());

			Assert.Same(monitor, first);
			Assert.Same(first, second);
			Assert.Equal(1, registry.Count);
			Assert.Equal(0, disposed);
		}

		[Fact]
		public void RemovingViewDisposesItsInteraction()
		{
			var disposed = 0;
			using var registry = new FoldableMonitorRegistry<object, object>(_ => disposed++);
			var view = new object();
			registry.GetOrAdd(view, () => new object());

			Assert.True(registry.Remove(view));
			Assert.Equal(1, disposed);
			Assert.Equal(0, registry.Count);
			Assert.False(registry.Remove(view));
		}

		[Fact]
		public void DisposingRegistryCleansUpAllInteractions()
		{
			var disposed = 0;
			var registry = new FoldableMonitorRegistry<object, object>(_ => disposed++);
			registry.GetOrAdd(new object(), () => new object());
			registry.GetOrAdd(new object(), () => new object());

			registry.Dispose();

			Assert.Equal(2, disposed);
			Assert.Equal(0, registry.Count);
		}
	}
}
