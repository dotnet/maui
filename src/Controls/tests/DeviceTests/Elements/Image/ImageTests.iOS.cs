using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageTests
	{
		const string coffeeBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAA4CAYAAAC7UXvqAAAABmJLR0QA/wD/AP+gvaeTAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH3gUJADAhwicxqAAAAB1pVFh0Q29tbWVudAAAAAAAQ3JlYXRlZCB3aXRoIEdJTVBkLmUHAAABTklEQVRo3u2ZvUoDQRSFv2g6FZ/A0p/G9PZpJCCWFr6BoAGfxsY3sTZgbAKRYCtBCwkh6aIQmyFGEJPZnd25Y86B6XaX882cvXOHAemHpm6UprXUZ2zlAPJGJHjEFCEBCEAAAlhtAEmSpLiqzLW5SfpPfh+oJpKQ3w5GaiUEIAABCEAAAhDAf++FpsuuwCT1CI0Nent13ehfYwbQNwjQ91mBnkGAJx+AlkGAts/DNb6vf6yMA1/iriHznSwb2a2h+NxkeWkTeDcw+2/ARlbypgGAi7ytRTui+XtgPW/+9oFRBPNDYDfUT3QCfJZofgI0QleC85IgPoCzosrZqWv0ijI/Ao6Lrsl7wGMB5h/ct0s7+FwBgwDGB8BlrMPUNnDtOkVf4123z2yFNFTJ8e4hUAeOXBR25syNgRfg2dX2O5/+JguA1RuahROsm/rY+gI8XGfJmDMlSQAAAABJRU5ErkJggg==";

		[Fact]
		public async Task ImageLoadViaInputStreamInitializesCorrectly()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var image = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(coffeeBase64)));

				var platformImage = await image.GetPlatformImageAsync(MauiContext);

				Assert.True(platformImage.Value.Size.Height > 0);
				Assert.True(platformImage.Value.Size.Width > 0);
			});
		}

		[Fact("Large images resize correctly")]
		public async Task LargeImagesResizeCorrectly()
		{
			SetupBuilder();
			var layout = new StackLayout { MaximumWidthRequest = 200 };
			var image = new Image
			{
				Background = Colors.Red,
				Source = "big_white_horizontal.png",
				Aspect = Aspect.AspectFit
			};

			layout.Add(image);

			// big_white_horizontal is 2000x1000px, so it should perfectly fit into a 200px wide layout
			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<LayoutHandler>(layout);

				await image.Wait();

				await handler.ToPlatform().AssertContainsColor(Colors.White, MauiContext);
				await handler.ToPlatform().AssertDoesNotContainColor(Colors.Red, MauiContext);
			});

			// We check for both image and layout to ensure that image is the right size
			Assert.True(image.Height == 100, "Image should be 100px tall after resize!");
			Assert.True(image.Width == 200, "Image should be 200px wide after resize!");

			Assert.True(layout.Height == 100, "Layout should be 100px tall!");
			Assert.True(layout.Width == 200, "Layout should be 200px wide!");
		}


		// big_white_horizontal is 2000x1000px
		// So when it has to fit a 50x200 space, it should resize to 50x25 (width is the smallest scaled dimension)
		// When it has to fit in a 200x50 space, it should resize to 100x50 (height is the smallest scaled dimension)
		[Theory("Aspect Fit resizes on smallest dimension")]
		[InlineData(50, 200, 50, 25)]
		[InlineData(200, 50, 100, 50)]
		public async Task AspectFitResizesOnSmallestDimensions(int widthRequest, int heightRequest, int expectedWidth, int expectedHeight)
		{
			SetupBuilder();
			var layout = new Grid { MaximumHeightRequest = heightRequest, MaximumWidthRequest = widthRequest };
			var image = new Image
			{
				Source = "big_white_horizontal.png",
				Aspect = Aspect.AspectFit
			};

			layout.Add(image);

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<LayoutHandler>(layout);

				await image.Wait();

				await handler.ToPlatform().AssertContainsColor(Colors.White, MauiContext);
			});

			Assert.True(image.Height == expectedHeight, $"Image should have expected height! Expected: {expectedHeight}, was: {image.Height}");
			Assert.True(image.Width == expectedWidth, $"Image should have expected width! Expected: {expectedWidth}, was: {image.Width}");

			Assert.True(layout.Height == expectedHeight, $"Layout should have expected height! Expected: {expectedHeight}, was: {layout.Height}");
			Assert.True(layout.Width == expectedWidth, $"Layout should have expected width! Expected: {expectedWidth}, was: {layout.Width}");
		}
	}
}