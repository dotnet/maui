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

				await image.WaitUntilLoaded();

				await handler.ToPlatform().AssertContainsColor(Colors.White, MauiContext);
				await handler.ToPlatform().AssertDoesNotContainColor(Colors.Red, MauiContext);
			});

			// We check for both image and layout to ensure that image is the right size
			Assert.Equal(100, image.Height);
			Assert.Equal(200, image.Width);

			Assert.Equal(100, layout.Height);
			Assert.Equal(200, layout.Width);
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

				await image.WaitUntilLoaded();

				await handler.ToPlatform().AssertContainsColor(Colors.White, MauiContext);
			});

			Assert.Equal(expectedHeight, image.Height);
			Assert.Equal(expectedWidth, image.Width);

			Assert.Equal(expectedHeight, layout.Height);
			Assert.Equal(expectedWidth, layout.Width);
		}

		[Fact]
		public async Task ImagesRespectExplicitConstraints()
		{
			SetupBuilder();

			var layout = new Grid();
			var image = new Image
			{
				Source = "big_white_horizontal.png",
				Aspect = Aspect.AspectFit,
				HeightRequest = 100
			};

			layout.Add(image);

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<LayoutHandler>(layout);

				await image.WaitUntilLoaded();

				await handler.ToPlatform().AssertContainsColor(Colors.White, MauiContext);
			});

			// We asked the image to have a fixed height, so it should resize accordingly even if it could grow in width
			Assert.Equal(100, image.Height);
			Assert.Equal(200, image.Width);
		}

		[Fact]
		public async Task ImageLoadsIfYouDontSpecifyExtension()
		{
			SetupBuilder();

			var layout = new Grid();
			var image = new Image
			{
				Source = "big_white_horizontal",
				Aspect = Aspect.AspectFit,
				HeightRequest = 100
			};

			layout.Add(image);

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<LayoutHandler>(layout);

				await image.WaitUntilLoaded();

				await handler.ToPlatform().AssertContainsColor(Colors.White, MauiContext);
			});

			// We asked the image to have a fixed height, so it should resize accordingly even if it could grow in width
			Assert.Equal(100, image.Height);
			Assert.Equal(200, image.Width);
		}
	}
}