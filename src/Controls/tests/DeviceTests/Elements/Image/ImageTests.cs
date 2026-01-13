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

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<LayoutHandler>(layout);
				await image.WaitUntilLoaded();
				
				// Diagnostic logging for Helix
				var platformView = handler.ToPlatform();
				System.Diagnostics.Debug.WriteLine($"[ImageTest] Layout size: {layout.Width}x{layout.Height}");
				System.Diagnostics.Debug.WriteLine($"[ImageTest] Image size: {image.Width}x{image.Height}");
				System.Diagnostics.Debug.WriteLine($"[ImageTest] Platform view size: {platformView.ActualWidth}x{platformView.ActualHeight}");
				System.Diagnostics.Debug.WriteLine($"[ImageTest] Image.IsLoaded: {image.IsLoaded}");
				
				await handler.ToPlatform().AssertContainsColor(Colors.Red, MauiContext);
			});
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