using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Image)]
	public partial class ImageTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Image, ImageHandler>();
				});
			});
		}

		[Fact]
		public async Task ImageWithUndefinedSizeAndWithBackgroundSetRenders()
		{
			SetupBuilder();
			var layout = new VerticalStackLayout();

			var image = new Image
			{
				Background = Colors.Black,
				Source = "red.png",
			};

			layout.Add(image);

#if WINDOWS || ANDROID
			// Host the layout in a window so loading and native layout run through the normal lifecycle.
			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async handler =>
			{
				await image.WaitUntilLoaded();
#if ANDROID
				var nativeImage = ((ImageHandler)image.Handler).PlatformView;
				await AssertHelpers.AssertEventually(
					() => nativeImage.Drawable != null &&
						nativeImage.Width > 0 && nativeImage.Height > 0 &&
						!nativeImage.IsLayoutRequested,
					message: "The loaded image did not complete native layout.");
#endif
				await handler.ToPlatform().AssertContainsColor(Colors.Red, handler.MauiContext);
			});
#else
			// On iOS/MacCatalyst, use the original approach to avoid timeout issues
			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<LayoutHandler>(layout);
				await image.WaitUntilLoaded();
				await handler.ToPlatform().AssertContainsColor(Colors.Red, MauiContext);
			});
#endif
		}

		[Fact]
		[Description("The BackgroundColor of a Image should match with native background color")]
		public async Task ImageBackgroundColorConsistent()
		{
			var expected = Colors.AliceBlue;
			var image = new Image()
			{
				BackgroundColor = expected,
				HeightRequest = 100,
				WidthRequest = 200
			};

			await ValidateHasColor(image, expected, typeof(ImageHandler));
		}
	}
}