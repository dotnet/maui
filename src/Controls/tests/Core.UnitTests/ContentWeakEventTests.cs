using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// A child can outlive the parent that hosted it -- cached by the app, or re-parented onto a
	// different host. A plain PropertyChanged handler on the child therefore keeps every parent it
	// was ever attached to alive, and with it that parent's whole subtree.
	public class ContentWeakEventTests : BaseTestFixture
	{
		// https://github.com/dotnet/maui/issues/37218
		[Fact]
		public async Task ReusedContentDoesNotRootSwipeView()
		{
			var shared = new Label { Text = "shared" };

			var reference = CreateSwipeView(shared);

			Assert.False(await reference.WaitForCollect(), "SwipeView should not be alive!");
			GC.KeepAlive(shared);
		}

		// https://github.com/dotnet/maui/issues/37217
		[Fact]
		public async Task ReusedPageDoesNotRootShellContent()
		{
			var shared = new ContentPage();

			var reference = CreateShellContent(shared);

			Assert.False(await reference.WaitForCollect(), "ShellContent should not be alive!");
			GC.KeepAlive(shared);
		}

		[Fact]
		public void SwipeViewStillTracksContentIsEnabled()
		{
			var content = new Label { Text = "a" };
			var swipe = new SwipeView { Content = content };

			// Attached content still notifies: flipping IsEnabled reaches the SwipeView's handler
			// path without throwing, and the content stays parented.
			content.IsEnabled = false;

			Assert.Same(swipe, content.Parent);
			GC.KeepAlive(swipe);
		}

		[Fact]
		public void RemovingContentDetachesIt()
		{
			var content = new Label { Text = "a" };
			var swipe = new SwipeView { Content = content };
			Assert.Same(swipe, content.Parent);

			swipe.Content = null;

			Assert.Null(content.Parent);
			GC.KeepAlive(swipe);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateSwipeView(View content) =>
			new WeakReference(new SwipeView { Content = content });

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateShellContent(Page page)
		{
			var sc = new ShellContent { Content = page };
			return new WeakReference(sc);
		}
	}
}
