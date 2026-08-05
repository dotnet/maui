using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class TitleBarTests : BaseTestFixture
	{
		[Fact]
		public void BackgroundColorBridgeNotifiesAfterReattachingTitleBar()
		{
			var window = new Window { Page = new ContentPage() };
			var titleBar = new TitleBar();
			window.TitleBar = titleBar;
			window.TitleBar = null;
			window.TitleBar = titleBar;

			var notifications = 0;
			titleBar.PropertyChanged += (_, args) =>
			{
				if (args.PropertyName == "BackgroundColorBridge")
					notifications++;
			};

#pragma warning disable CS0618 // BackgroundColor — verifies backward-compatible template updates
			titleBar.BackgroundColor = Colors.Red;
#pragma warning restore CS0618

			Assert.Equal(1, notifications);
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task TitleBarDoesNotLeak()
		{
			var application = new Application();

			WeakReference CreateReference()
			{
				var window = new Window { Page = new ContentPage() };
				var firstTitleBar = new TitleBar();
				var secondTitleBar = new TitleBar();
				var reference = new WeakReference(firstTitleBar);

				window.TitleBar = firstTitleBar;

				application.OpenWindow(window);

				window.TitleBar = secondTitleBar;

				((IWindow)window).Destroying();
				return reference;
			}

			var reference = CreateReference();

			// GC
			await TestHelpers.Collect();

			Assert.False(reference.IsAlive, "TitleBar should not be alive!");

			GC.KeepAlive(application);
		}
	}
}