using System.Threading.Tasks;
using AndroidX.Core.View;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;
using Xunit;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Layout)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue37418 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ContentRemainsTopAlignedAfterTranslationReturnsOnScreen()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddMauiControlsHandlers();
					handlers.AddHandler(typeof(Window), typeof(WindowHandlerStub));
				});
			});

			var firstChild = new Button
			{
				Text = "Close"
			};
			var content = new VerticalStackLayout
			{
				Padding = 0,
				Spacing = 16,
				Children =
				{
					firstChild,
					new Label
					{
						Text = "Bottom Sheet Content",
						FontSize = 28
					},
					new Button
					{
						Text = "Custom button",
						Margin = new Thickness(16, 0)
					}
				}
			};
			var bottomSheet = new Border
			{
				HeightRequest = 340,
				Padding = 0,
				VerticalOptions = LayoutOptions.End,
				Content = content
			};
			var page = new ContentPage
			{
				Content = new Grid
				{
					bottomSheet
				}
			};
			await CreateHandlerAndAddToWindow(page, async () =>
			{
				var contentPlatformView = (AView)content.Handler.PlatformView;
				var rootInsets = ViewCompat.GetRootWindowInsets(contentPlatformView);
				Assert.NotNull(rootInsets);

				bottomSheet.TranslationY = page.Height + bottomSheet.HeightRequest;
				ViewCompat.DispatchApplyWindowInsets(contentPlatformView, rootInsets);

				bottomSheet.TranslationY = 0;

				Assert.True(
					contentPlatformView.PaddingTop == 0,
					"Bottom sheet content should remain aligned to the top after resetting TranslationY.");
				await Task.CompletedTask;
			});
		}
	}
}
