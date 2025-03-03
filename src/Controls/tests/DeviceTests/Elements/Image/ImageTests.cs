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
				await handler.ToPlatform().AssertContainsColor(Colors.Red, MauiContext);
			});
		}

		// NOTE: this test is slightly different than MemoryTests.HandlerDoesNotLeak
		// It sets image.Source and waits for it to load, a valid test case.
		[Fact("Image Does Not Leak")]
		public async Task DoesNotLeak()
		{
			SetupBuilder();
			WeakReference platformViewReference = null;
			WeakReference handlerReference = null;

			await InvokeOnMainThreadAsync(async () =>
			{
				var layout = new VerticalStackLayout();
				var image = new Image
				{
					Background = Colors.Black,
					Source = "red.png",
				};
				layout.Add(image);

				var handler = CreateHandler<LayoutHandler>(layout);
				handlerReference = new WeakReference(image.Handler);
				platformViewReference = new WeakReference(image.Handler.PlatformView);
				await image.WaitUntilLoaded();
			});

			await AssertionExtensions.WaitForGC(handlerReference, platformViewReference);
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