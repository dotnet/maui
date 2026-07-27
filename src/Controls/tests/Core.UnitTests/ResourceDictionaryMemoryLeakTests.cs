using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ResourceDictionaryMemoryLeakTests : BaseTestFixture
	{
		/// <summary>
		/// Verifies that merging a long-lived (rooted) <see cref="ResourceDictionary"/> into a
		/// transient element's <c>Resources.MergedDictionaries</c> does not retain that element.
		/// Reproduces issue #36308: the parent dictionary subscribes to the child's
		/// <c>ValuesChanged</c> event with a strong delegate, so a rooted child dictionary pins the
		/// transient element (and its whole subtree) for the dictionary's lifetime with no unload teardown.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task MergedRootedResourceDictionaryDoesNotLeakElement()
		{
			// A ResourceDictionary rooted by something longer-lived than the element
			// (e.g. a static field or singleton reused across pages).
			var sharedDictionary = new ResourceDictionary { { "primary", Colors.Red } };

			WeakReference CreateElementReference()
			{
				var element = new ContentView();
				element.Resources.MergedDictionaries.Add(sharedDictionary);
				return new WeakReference(element);
			}

			var reference = CreateElementReference();

			Assert.False(await reference.WaitForCollect(), "ContentView should be collected, but it was retained by the shared ResourceDictionary's ValuesChanged subscription.");

			// Keep the shared dictionary alive for the duration of the test.
			GC.KeepAlive(sharedDictionary);
		}
	}
}
