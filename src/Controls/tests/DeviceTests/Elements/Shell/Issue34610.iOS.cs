#if IOS
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue34610 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task TitleViewMatchesPageContentWidth()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);
				});
			});

			var titleView = new Grid
			{
				BackgroundColor = Colors.Red,
				HorizontalOptions = LayoutOptions.Fill,
				Margin = 0,
				Padding = 0,
				Children =
				{
					new Label
					{
						Text = "MY APP TITLE",
						VerticalOptions = LayoutOptions.Center
					}
				}
			};

			var content = new Grid
			{
				BackgroundColor = Colors.Blue
			};

			var page = new ContentPage
			{
				Content = content,
				Padding = 0
			};

			Shell.SetTitleView(page, titleView);

			var shell = new Shell
			{
				CurrentItem = page,
				FlyoutBehavior = FlyoutBehavior.Disabled
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(shell), async (handler) =>
			{
				await OnFrameSetToNotEmpty(titleView);
				await OnFrameSetToNotEmpty(content);

				Assert.True(
					Math.Abs(content.Width - titleView.Width) < 1,
					$"Shell TitleView should match page content width. Expected: {content.Width}, Actual: {titleView.Width}.");
			});
		}
	}
}
#endif
