using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowHandlerTests : CoreHandlerTestBase
	{
#if MACCATALYST
		[Fact]
		public async Task ContentIsSetInitially()
		{
			var window = new Window
			{
				Page = new ContentPage
				{
					Content = new Button { Text = "Yay!" }
				}
			};

			await RunWindowTest(window, handler =>
			{
				var root = handler.PlatformView.RootViewController;

				Assert.NotNull(root);
				var page = Assert.IsType<PageViewController>(root);

				Assert.NotNull(page.View);
				var content = Assert.IsType<Platform.ContentView>(root.View.Subviews[0]);
				var btn = Assert.IsType<UIButton>(content.Subviews[0]);

				Assert.Equal("Yay!", btn.Title(UIControlState.Normal));
			});
		}

		[Fact]
		public async Task WindowSupportsEmptyPage_Platform()
		{
			var window = new Window(new ContentPage());

			await RunWindowTest(window, handler =>
			{
				var root = handler.PlatformView.RootViewController;

				Assert.NotNull(root);
				var page = Assert.IsType<PageViewController>(root);

				Assert.NotNull(page.View);
				var content = Assert.IsType<Platform.ContentView>(root.View.Subviews[0]);

				Assert.Empty(content.Subviews);
			});
		}

		void MovePlatformWindow(UIWindow window, Rect rect)
		{
			window.SetFrame(rect, true, false);
		}
#endif
	}
}