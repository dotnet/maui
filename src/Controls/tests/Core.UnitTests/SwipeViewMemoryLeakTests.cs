using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class SwipeViewMemoryLeakTests : BaseTestFixture
	{
		/// <summary>
		/// Verifies that a <see cref="SwipeView"/> placed beneath a long-lived <see cref="ScrollView"/>
		/// does not leak after it is detached from the scroll container by removing an intermediate
		/// ancestor (which leaves the SwipeView's direct parent unchanged). Reproduces issue #36481:
		/// the ancestor <c>Scrolled</c> subscription was a plain (non-weak) delegate, so the long-lived
		/// ScrollView permanently rooted the detached SwipeView and its subtree.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task SwipeViewDoesNotLeakWhenAncestorScrollViewOutlivesIt()
		{
			// The ScrollView is the long-lived root that outlives the SwipeView.
			var scroll = new ScrollView();

			WeakReference CreateSwipeViewReference()
			{
				var inner = new VerticalStackLayout();
				scroll.Content = inner;

				var swipe = new SwipeView { Content = new Label() };
				inner.Children.Add(swipe);   // subscribes to scroll.Scrolled

				// Detach the intermediate ancestor: swipe.Parent stays 'inner', so the
				// direct-parent-change teardown never runs.
				scroll.Content = null;

				return new WeakReference(swipe);
			}

			var reference = CreateSwipeViewReference();

			Assert.False(await reference.WaitForCollect(), "SwipeView should not be alive!");

			// Keep the long-lived ScrollView alive for the duration of the test.
			GC.KeepAlive(scroll);
		}
	}
}
