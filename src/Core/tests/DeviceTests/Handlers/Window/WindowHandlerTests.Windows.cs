using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Windows.Graphics;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowHandlerTests : HandlerTestBase
	{
		[Fact]
		public async Task ContentIsSetInitially()
		{
			var window = new Window
			{
				Page = new ContentPage
				{
					Content = new Label { Text = "Yay!" }
				}
			};

			await RunWindowTest(window, handler =>
			{
				var rootContainer = handler.PlatformView.Content;

				Assert.NotNull(rootContainer);
				var container = Assert.IsType<WindowRootViewContainer>(rootContainer);

				Assert.NotEmpty(container.Children);
				var root = Assert.IsType<WindowRootView>(container.Children[0]);
				var navigation = Assert.IsType<RootNavigationView>(root.Content);
				var panel = Assert.IsType<ContentPanel>(navigation.Content);
				var btn = Assert.IsAssignableFrom<UI.Xaml.Controls.TextBlock>(panel.Children[0]);

				Assert.Equal("Yay!", btn.Text);
			});
		}

		[Fact]
		public async Task WindowSupportsEmptyPage_Platform()
		{
			var window = new Window(new ContentPage());

			await RunWindowTest(window, handler =>
			{
				var rootContainer = handler.PlatformView.Content;

				Assert.NotNull(rootContainer);
				var container = Assert.IsType<WindowRootViewContainer>(rootContainer);

				Assert.NotEmpty(container.Children);
				var root = Assert.IsType<WindowRootView>(container.Children[0]);
				var navigation = Assert.IsType<RootNavigationView>(root.Content);
				var panel = Assert.IsType<ContentPanel>(navigation.Content);

				Assert.Null(panel.Content);
				Assert.Empty(panel.Children);
			});
		}

		void MovePlatformWindow(UI.Xaml.Window window, Rect rect)
		{
			var density = window.GetDisplayDensity();
			window.GetAppWindow().MoveAndResize(new RectInt32(
				(int)(rect.X * density),
				(int)(rect.Y * density),
				(int)(rect.Width * density),
				(int)(rect.Height * density)));
		}
	}
}